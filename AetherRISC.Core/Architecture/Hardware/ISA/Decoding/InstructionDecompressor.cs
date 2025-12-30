using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

public static class InstructionDecompressor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCompressed(ushort inst16) => (inst16 & 0x3) != 0x3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Decompress(ushort inst16)
    {
        if (!IsCompressed(inst16))
            throw new ArgumentException("Not a compressed instruction", nameof(inst16));

        if (inst16 == 0) return 0xFFFFFFFFu;

        int quadrant = inst16 & 0x3;
        int funct3 = (inst16 >> 13) & 0x7;

        return quadrant switch
        {
            1 => DecompressQuadrant1(inst16, funct3),
            2 => DecompressQuadrant2(inst16, funct3),
            _ => 0xFFFFFFFFu
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressQuadrant1(ushort c, int funct3)
    {
        return funct3 switch
        {
            0b000 => DecompressCAddiOrNop(c),
            0b010 => DecompressCLi(c),
            0b011 => DecompressCLuiOrAddi16Sp(c),
            0b101 => DecompressCJ(c),
            0b110 => DecompressCBeqz(c),
            0b111 => DecompressCBnez(c),
            _ => 0xFFFFFFFFu
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressQuadrant2(ushort c, int funct3)
    {
        if (c == 0x9002) return 0x00100073u; // C.EBREAK

        return funct3 switch
        {
            0b100 => DecompressCArithJrJalrMvAdd(c),
            _ => 0xFFFFFFFFu
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressCAddiOrNop(ushort c)
    {
        int rd = (c >> 7) & 0x1F;
        int imm = SignExtend(((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5), 6);
        return EncodeAddi(rd, rd, imm);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressCLi(ushort c)
    {
        int rd = (c >> 7) & 0x1F;
        int imm = SignExtend(((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5), 6);
        return EncodeAddi(rd, 0, imm);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressCLuiOrAddi16Sp(ushort c)
    {
        int rd = (c >> 7) & 0x1F;
        int nzimm = SignExtend((((c >> 2) & 0x1F) | (((c >> 12) & 0x1) << 5)) << 12, 18);
        if (rd == 2) return 0xFFFFFFFFu;
        return EncodeLui(rd, nzimm);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressCJ(ushort c) => EncodeJal(0, DecodeCJOffset(c));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressCBeqz(ushort c)
    {
        int rs1 = 8 + ((c >> 7) & 0x7);
        return EncodeBeq(rs1, 0, DecodeCBOffset(c));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressCBnez(ushort c)
    {
        int rs1 = 8 + ((c >> 7) & 0x7);
        return EncodeBne(rs1, 0, DecodeCBOffset(c));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint DecompressCArithJrJalrMvAdd(ushort c)
    {
        int bit12 = (c >> 12) & 0x1;
        int rdRs1 = (c >> 7) & 0x1F;
        int rs2 = (c >> 2) & 0x1F;

        if (rs2 == 0) {
            if (rdRs1 == 0) return 0xFFFFFFFFu;
            int rd = bit12 == 0 ? 0 : 1;
            return EncodeJalr(rd, rdRs1, 0);
        }

        if (rdRs1 == 0) return 0xFFFFFFFFu;

        if (bit12 == 0) return EncodeAdd(rdRs1, 0, rs2); // MV
        return EncodeAdd(rdRs1, rdRs1, rs2); // ADD
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeCJOffset(ushort c)
    {
        int imm11 = (c >> 12) & 0x1;
        int imm4 = (c >> 11) & 0x1;
        int imm9_8 = (c >> 9) & 0x3;
        int imm10 = (c >> 8) & 0x1;
        int imm6 = (c >> 7) & 0x1;
        int imm7 = (c >> 6) & 0x1;
        int imm3_1 = (c >> 3) & 0x7;
        int imm5 = (c >> 2) & 0x1;
        int imm = (imm11 << 11) | (imm10 << 10) | (imm9_8 << 8) | (imm7 << 7) | (imm6 << 6) | (imm5 << 5) | (imm4 << 4) | (imm3_1 << 1);
        return SignExtend(imm, 12);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeCBOffset(ushort c)
    {
        int imm8 = (c >> 12) & 0x1;
        int imm7_6 = (c >> 10) & 0x3;
        int imm5 = (c >> 2) & 0x1;
        int imm4_3 = (c >> 5) & 0x3;
        int imm2_1 = (c >> 3) & 0x3;
        int imm = (imm8 << 8) | (imm7_6 << 6) | (imm5 << 5) | (imm4_3 << 3) | (imm2_1 << 1);
        return SignExtend(imm, 9);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SignExtend(int value, int bits)
    {
        int shift = 32 - bits;
        return (value << shift) >> shift;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeAddi(int rd, int rs1, int imm) => ((uint)imm & 0xFFFu) << 20 | ((uint)rs1 << 15) | ((uint)rd << 7) | 0x13u;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeLui(int rd, int imm) => ((uint)imm & 0xFFFFF000u) | ((uint)rd << 7) | 0x37u;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeAdd(int rd, int rs1, int rs2) => ((uint)rs2 << 20) | ((uint)rs1 << 15) | ((uint)rd << 7) | 0x33u;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeJal(int rd, int imm) { uint u = (uint)imm; return (((u>>20)&1)<<31) | (((u>>1)&0x3FF)<<21) | (((u>>11)&1)<<20) | (((u>>12)&0xFF)<<12) | ((uint)rd << 7) | 0x6Fu; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeJalr(int rd, int rs1, int imm) => ((uint)imm & 0xFFFu) << 20 | ((uint)rs1 << 15) | ((uint)rd << 7) | 0x67u;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeBeq(int rs1, int rs2, int imm) { uint u = (uint)imm; return (((u>>12)&1)<<31) | (((u>>5)&0x3F)<<25) | ((uint)rs2<<20) | ((uint)rs1<<15) | (((u>>1)&0xF)<<8) | (((u>>11)&1)<<7) | 0x63u; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeBne(int rs1, int rs2, int imm) { uint u = (uint)imm; return (((u>>12)&1)<<31) | (((u>>5)&0x3F)<<25) | ((uint)rs2<<20) | ((uint)rs1<<15) | (1u<<12) | (((u>>1)&0xF)<<8) | (((u>>11)&1)<<7) | 0x63u; }
}
