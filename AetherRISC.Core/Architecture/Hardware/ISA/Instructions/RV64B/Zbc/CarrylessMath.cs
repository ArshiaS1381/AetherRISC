using System;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc;

public static class CarrylessMath
{
    public static (ulong lo, ulong hi) ClmulLoHi(ulong rs1, ulong rs2, int xlen)
    {
        ulong lo = 0;
        ulong hi = 0;
        for (int i = 0; i < xlen; i++)
        {
            if (((rs2 >> i) & 1ul) != 0)
            {
                lo ^= (rs1 << i);
                if (i > 0) hi ^= (rs1 >> (xlen - i));
            }
        }
        return (lo, hi);
    }

    public static ulong Clmulr(ulong rs1, ulong rs2, int xlen)
    {
        ulong res = 0;
        for (int i = 0; i < xlen; i++)
        {
            if (((rs2 >> i) & 1ul) != 0) res ^= (rs1 >> (xlen - i - 1));
        }
        return res;
    }
}

