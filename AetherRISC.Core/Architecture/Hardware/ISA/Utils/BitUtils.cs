using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Utils
{
    public static class BitUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SignExtend(int value, int bits)
        {
            int shift = 32 - bits;
            return (value << shift) >> shift;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractITypeImm(uint inst)
        {
            // inst[31:20]
            return SignExtend((int)(inst >> 20), 12);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractSTypeImm(uint inst)
        {
            // inst[31:25] | inst[11:7]
            int imm = (int)(((inst >> 25) << 5) | ((inst >> 7) & 0x1F));
            return SignExtend(imm, 12);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractBTypeImm(uint inst)
        {
            // inst[31], inst[7], inst[30:25], inst[11:8]
            int imm = (int)(((inst >> 31) & 1) << 12)
                    | (int)(((inst >> 7) & 1) << 11)
                    | (int)(((inst >> 25) & 0x3F) << 5)
                    | (int)(((inst >> 8) & 0xF) << 1);
            return SignExtend(imm, 13);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractUTypeImm(uint inst)
        {
            // inst[31:12] << 12
            return (int)(inst & 0xFFFFF000);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractJTypeImm(uint inst)
        {
             // inst[31], inst[19:12], inst[20], inst[30:21]
             int imm = (int)(((inst >> 31) & 1) << 20)
                     | (int)(((inst >> 12) & 0xFF) << 12)
                     | (int)(((inst >> 20) & 1) << 11)
                     | (int)(((inst >> 21) & 0x3FF) << 1);
             return SignExtend(imm, 21);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractShamt(uint inst, int xlen)
        {
             int shamtBits = (xlen == 64) ? 6 : 5;
             int mask = (1 << shamtBits) - 1;
             return (int)((inst >> 20) & (uint)mask);
        }
    }
}
