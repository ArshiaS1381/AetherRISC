using AetherRISC.Core.Architecture.Hardware.ISA;
using System;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

public static class InstructionDecompressor
{
    public static bool IsCompressed(ushort inst16) => (inst16 & 0x3) != 0x3;

    public static uint Decompress(ushort inst16)
    {
        // If it isn't compressed, caller shouldn't be here.
        if (!IsCompressed(inst16))
            throw new ArgumentException("Not a compressed instruction", nameof(inst16));

        int quadrant = inst16 & 0x3;
        int funct3 = (inst16 >> 13) & 0x7;

        return quadrant switch
        {
            1 => DecompressQuadrant1(inst16, funct3),
            2 => DecompressQuadrant2(inst16, funct3),
            _ => 0x00000013u // treat unsupported as NOP
        };
    }

    private static uint DecompressQuadrant1(ushort c, int funct3)
    {
        // Quadrant 1: opcode bits[1:0] == 01
        return funct3 switch
        {
            0b000 => DecompressCAddiOrNop(c),
            0b010 => DecompressCLi(c),
            0b011 => DecompressCLuiOrAddi16Sp(c),
            0b101 => DecompressCJ(c),
            0b110 => DecompressCBeqz(c),
            0b111 => DecompressCBnez(c),
            _ => 0x00000013u
        };
    }

    private static uint DecompressQuadrant2(ushort c, int funct3)
    {
        // Quadrant 2: opcode bits[1:0] == 10
        if (c == 0x9002) // canonical C.EBREAK
            return 0x00100073u;

        return funct3 switch
        {
            0b100 => DecompressCArithJrJalrMvAdd(c),
            _ => 0x00000013u
        };
    }

    private static uint DecompressCAddiOrNop(ushort c)
    {
        int rd = (c >> 7) & 0x1F;
        int imm = SignExtend(((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5), 6);

        // C.NOP is C.ADDI x0, 0
        return EncodeAddi(rd, rd, imm);
    }

    private static uint DecompressCLi(ushort c)
    {
        int rd = (c >> 7) & 0x1F;
        int imm = SignExtend(((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5), 6);
        return EncodeAddi(rd, 0, imm);
    }

    private static uint DecompressCLuiOrAddi16Sp(ushort c)
    {
        int rd = (c >> 7) & 0x1F;
        int nzimm = SignExtend(
            (((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5)) << 12,
            18
        );

        // rd==2 would be C.ADDI16SP, which we don't implement yet -> NOP.
        if (rd == 2)
            return 0x00000013u;

        return EncodeLui(rd, nzimm);
    }

    private static uint DecompressCJ(ushort c)
    {
        int off = DecodeCJOffset(c);
        return EncodeJal(0, off);
    }

    private static uint DecompressCBeqz(ushort c)
    {
        int rs1p = (c >> 7) & 0x7; // 3-bit
        int rs1 = 8 + rs1p;
        int off = DecodeCBOffset(c);
        return EncodeBeq(rs1, 0, off);
    }

    private static uint DecompressCBnez(ushort c)
    {
        int rs1p = (c >> 7) & 0x7; // 3-bit
        int rs1 = 8 + rs1p;
        int off = DecodeCBOffset(c);
        return EncodeBne(rs1, 0, off);
    }

    private static uint DecompressCArithJrJalrMvAdd(ushort c)
    {
        int bit12 = (c >> 12) & 0x1;
        int rdRs1 = (c >> 7) & 0x1F;
        int rs2 = (c >> 2) & 0x1F;

        // If rs2 == 0, this is JR/JALR/EBREAK
        if (rs2 == 0)
        {
            if (rdRs1 == 0)
                return 0x00000013u; // reserved (other than 0x9002 ebreak handled earlier)

            // JR (bit12=0): jalr x0, rdRs1, 0
            // JALR (bit12=1): jalr x1, rdRs1, 0
            int rd = bit12 == 0 ? 0 : 1;
            return EncodeJalr(rd, rdRs1, 0);
        }

        // Otherwise, this is MV/ADD
        if (rdRs1 == 0)
            return 0x00000013u;

        if (bit12 == 0)
        {
            // C.MV: add rd, x0, rs2
            return EncodeAdd(rdRs1, 0, rs2);
        }

        // C.ADD: add rd, rd, rs2
        return EncodeAdd(rdRs1, rdRs1, rs2);
    }

    private static int DecodeCJOffset(ushort c)
    {
        // Build imm[11:1], then <<1, sign-extend 12 bits (imm[11] is sign)
        int imm11 = (c >> 12) & 0x1;
        int imm4 = (c >> 11) & 0x1;
        int imm9_8 = (c >> 9) & 0x3;
        int imm10 = (c >> 8) & 0x1;
        int imm6 = (c >> 7) & 0x1;
        int imm7 = (c >> 6) & 0x1;
        int imm3_1 = (c >> 3) & 0x7;
        int imm5 = (c >> 2) & 0x1;

        int imm =
            (imm11 << 11)
            | (imm10 << 10)
            | (imm9_8 << 8)
            | (imm7 << 7)
            | (imm6 << 6)
            | (imm5 << 5)
            | (imm4 << 4)
            | (imm3_1 << 1);

        int off = SignExtend(imm, 12);
        return off;
    }

    private static int DecodeCBOffset(ushort c)
    {
        // Build imm[8:1], then <<1, sign-extend 9 bits (imm[8] is sign)
        int imm8 = (c >> 12) & 0x1;
        int imm7_6 = (c >> 10) & 0x3;
        int imm5 = (c >> 2) & 0x1;
        int imm4_3 = (c >> 5) & 0x3;
        int imm2_1 = (c >> 3) & 0x3;

        int imm =
            (imm8 << 8) | (imm7_6 << 6) | (imm5 << 5) | (imm4_3 << 3) | (imm2_1 << 1);

        int off = SignExtend(imm, 9);
        return off;
    }

    private static int SignExtend(int value, int bits)
    {
        int shift = 32 - bits;
        return (value << shift) >> shift;
    }

    // Minimal encoders (match your InstructionEncoder bit layouts)

    private static uint EncodeAddi(int rd, int rs1, int imm)
    {
        uint uimm = (uint)imm & 0xFFFu;
        return (uimm << 20) | ((uint)rs1 << 15) | (0u << 12) | ((uint)rd << 7) | 0x13u;
    }

    private static uint EncodeLui(int rd, int imm)
    {
        return ((uint)imm & 0xFFFFF000u) | ((uint)rd << 7) | 0x37u;
    }

    private static uint EncodeAdd(int rd, int rs1, int rs2)
    {
        return (0u << 25)
            | ((uint)rs2 << 20)
            | ((uint)rs1 << 15)
            | (0u << 12)
            | ((uint)rd << 7)
            | 0x33u;
    }

    private static uint EncodeJal(int rd, int imm)
    {
        uint uimm = (uint)imm;

        return (((uimm >> 20) & 1u) << 31)
            | (((uimm >> 1) & 0x3FFu) << 21)
            | (((uimm >> 11) & 1u) << 20)
            | (((uimm >> 12) & 0xFFu) << 12)
            | ((uint)rd << 7)
            | 0x6Fu;
    }

    private static uint EncodeJalr(int rd, int rs1, int imm)
    {
        uint uimm = (uint)imm & 0xFFFu;
        return (uimm << 20) | ((uint)rs1 << 15) | (0u << 12) | ((uint)rd << 7) | 0x67u;
    }

    private static uint EncodeBeq(int rs1, int rs2, int imm)
    {
        uint uimm = (uint)imm;
        return (((uimm >> 12) & 1u) << 31)
            | (((uimm >> 5) & 0x3Fu) << 25)
            | ((uint)rs2 << 20)
            | ((uint)rs1 << 15)
            | (0u << 12)
            | (((uimm >> 1) & 0xFu) << 8)
            | (((uimm >> 11) & 1u) << 7)
            | 0x63u;
    }

    private static uint EncodeBne(int rs1, int rs2, int imm)
    {
        uint uimm = (uint)imm;
        return (((uimm >> 12) & 1u) << 31)
            | (((uimm >> 5) & 0x3Fu) << 25)
            | ((uint)rs2 << 20)
            | ((uint)rs1 << 15)
            | (1u << 12)
            | (((uimm >> 1) & 0xFu) << 8)
            | (((uimm >> 11) & 1u) << 7)
            | 0x63u;
    }
}

