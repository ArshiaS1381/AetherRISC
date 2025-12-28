using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64C;

public static class InstructionDecompressor
{
    public static bool IsCompressed(ushort inst16) => (inst16 & 0x3) != 0x3;

    public static uint Decompress(ushort inst16)
    {
        if (!IsCompressed(inst16))
            throw new ArgumentException("Not a compressed instruction", "inst16");

        int quadrant = inst16 & 0x3;
        int funct3 = (inst16 >> 13) & 0x7;

        switch (quadrant)
        {
            case 1: return DecompressQuadrant1(inst16, funct3);
            case 2: return DecompressQuadrant2(inst16, funct3);
            default: return 0x00000013u;
        }
    }

    private static uint DecompressQuadrant1(ushort c, int funct3)
    {
        switch (funct3)
        {
            case 0b000: return DecompressCAddiOrNop(c);
            case 0b010: return DecompressCLi(c);
            case 0b011: return DecompressCLuiOrAddi16Sp(c);
            case 0b101: return DecompressCJ(c);
            case 0b110: return DecompressCBeqz(c);
            case 0b111: return DecompressCBnez(c);
            default: return 0x00000013u;
        }
    }

    private static uint DecompressQuadrant2(ushort c, int funct3)
    {
        if (c == 0x9002) return 0x00100073u;
        switch (funct3)
        {
            case 0b100: return DecompressCArithJrJalrMvAdd(c);
            default: return 0x00000013u;
        }
    }

    private static uint DecompressCAddiOrNop(ushort c)
    {
        int rd = (c >> 7) & 0x1F;
        int imm = SignExtend(((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5), 6);
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
        int nzimm = SignExtend((((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5)) << 12, 18);
        if (rd == 2) return 0x00000013u;
        return EncodeLui(rd, nzimm);
    }

    private static uint DecompressCJ(ushort c)
    {
        int off = DecodeCJOffset(c);
        return EncodeJal(0, off);
    }

    private static uint DecompressCBeqz(ushort c)
    {
        int rs1 = 8 + ((c >> 7) & 0x7);
        int off = DecodeCBOffset(c);
        return EncodeBeq(rs1, 0, off);
    }

    private static uint DecompressCBnez(ushort c)
    {
        int rs1 = 8 + ((c >> 7) & 0x7);
        int off = DecodeCBOffset(c);
        return EncodeBne(rs1, 0, off);
    }

    private static uint DecompressCArithJrJalrMvAdd(ushort c)
    {
        int bit12 = (c >> 12) & 0x1;
        int rdRs1 = (c >> 7) & 0x1F;
        int rs2 = (c >> 2) & 0x1F;
        if (rs2 == 0)
        {
            if (rdRs1 == 0) return 0x00000013u;
            int rd = bit12 == 0 ? 0 : 1;
            return EncodeJalr(rd, rdRs1, 0);
        }
        if (rdRs1 == 0) return 0x00000013u;
        if (bit12 == 0) return EncodeAdd(rdRs1, 0, rs2);
        return EncodeAdd(rdRs1, rdRs1, rs2);
    }

    private static int DecodeCJOffset(ushort c)
    {
        int imm = ((c >> 12) & 0x1) << 11 | ((c >> 8) & 0x1) << 10 | ((c >> 9) & 0x3) << 8 | ((c >> 6) & 0x1) << 7 | ((c >> 7) & 0x1) << 6 | ((c >> 2) & 0x1) << 5 | ((c >> 11) & 0x1) << 4 | ((c >> 3) & 0x7) << 1;
        return SignExtend(imm, 12);
    }

    private static int DecodeCBOffset(ushort c)
    {
        int imm = ((c >> 12) & 0x1) << 8 | ((c >> 10) & 0x3) << 6 | ((c >> 2) & 0x1) << 5 | ((c >> 5) & 0x3) << 3 | ((c >> 3) & 0x3) << 1;
        return SignExtend(imm, 9);
    }

    private static int SignExtend(int value, int bits)
    {
        int shift = 32 - bits;
        return (value << shift) >> shift;
    }

    private static uint EncodeAddi(int rd, int rs1, int imm) => ((uint)imm & 0xFFFu) << 20 | (uint)rs1 << 15 | 0u << 12 | (uint)rd << 7 | 0x13u;
    private static uint EncodeLui(int rd, int imm) => ((uint)imm & 0xFFFFF000u) | (uint)rd << 7 | 0x37u;
    private static uint EncodeAdd(int rd, int rs1, int rs2) => 0u << 25 | (uint)rs2 << 20 | (uint)rs1 << 15 | 0u << 12 | (uint)rd << 7 | 0x33u;
    private static uint EncodeJal(int rd, int imm)
    {
        uint u = (uint)imm;
        return ((u >> 20) & 1u) << 31 | ((u >> 1) & 0x3FFu) << 21 | ((u >> 11) & 1u) << 20 | ((u >> 12) & 0xFFu) << 12 | (uint)rd << 7 | 0x6Fu;
    }
    private static uint EncodeJalr(int rd, int rs1, int imm) => ((uint)imm & 0xFFFu) << 20 | (uint)rs1 << 15 | 0u << 12 | (uint)rd << 7 | 0x67u;
    private static uint EncodeBeq(int rs1, int rs2, int imm)
    {
        uint u = (uint)imm;
        return ((u >> 12) & 1u) << 31 | ((u >> 5) & 0x3Fu) << 25 | (uint)rs2 << 20 | (uint)rs1 << 15 | 0u << 12 | ((u >> 1) & 0xFu) << 8 | ((u >> 11) & 1u) << 7 | 0x63u;
    }
    private static uint EncodeBne(int rs1, int rs2, int imm)
    {
        uint u = (uint)imm;
        return ((u >> 12) & 1u) << 31 | ((u >> 5) & 0x3Fu) << 25 | (uint)rs2 << 20 | (uint)rs1 << 15 | 1u << 12 | ((u >> 1) & 0xFu) << 8 | ((u >> 11) & 1u) << 7 | 0x63u;
    }
}


