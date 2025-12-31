using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Utils
{
    public static class BitUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SignExtend(uint value, int bits)
        {
            int shift = 32 - bits;
            return ((int)value << shift) >> shift;
        }

        public static int ExtractITypeImm(uint raw)
        {
            return SignExtend(raw >> 20, 12);
        }

        public static int ExtractSTypeImm(uint raw)
        {
            uint imm11_5 = (raw >> 25) & 0x7F;
            uint imm4_0 = (raw >> 7) & 0x1F;
            return SignExtend((imm11_5 << 5) | imm4_0, 12);
        }

        public static int ExtractBTypeImm(uint raw)
        {
            uint imm12 = (raw >> 31) & 0x1;
            uint imm10_5 = (raw >> 25) & 0x3F;
            uint imm4_1 = (raw >> 8) & 0xF;
            uint imm11 = (raw >> 7) & 0x1;
            
            uint imm = (imm12 << 12) | (imm11 << 11) | (imm10_5 << 5) | (imm4_1 << 1);
            // Bit 0 is always 0 for branches, but we extract as is. The format effectively multiplies by 2.
            // Actually RISC-V B-immediate encodes bits 12|10:5|4:1|11. 
            // The result is a 13-bit signed offset (multiples of 2).
            return SignExtend(imm, 13);
        }

        public static int ExtractUTypeImm(uint raw)
        {
            // U-type puts imm[31:12] into rd.
            // The immediate itself is the upper 20 bits, signed 32-bit.
            return (int)(raw & 0xFFFFF000);
        }

        public static int ExtractJTypeImm(uint raw)
        {
            uint imm20 = (raw >> 31) & 0x1;
            uint imm10_1 = (raw >> 21) & 0x3FF;
            uint imm11 = (raw >> 20) & 0x1;
            uint imm19_12 = (raw >> 12) & 0xFF;

            uint imm = (imm20 << 20) | (imm19_12 << 12) | (imm11 << 11) | (imm10_1 << 1);
            return SignExtend(imm, 21);
        }
        
        public static int ExtractShamt(uint raw, int xlen)
        {
            return (int)((raw >> 20) & (xlen == 64 ? 0x3F : 0x1F));
        }
    }
}
