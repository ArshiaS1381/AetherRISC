namespace AetherRISC.Core.Helpers
{
    public static class BitUtils
    {
        public static int ExtractITypeImm(uint inst)
        {
            unchecked 
            {
                // Cast to int first to ensure sign extension happens naturally if needed,
                // or just shift raw bits. For RISC-V I-Type, the top 12 bits are the immediate.
                return (int)((int)inst >> 20);
            }
        }

        public static int ExtractSTypeImm(uint inst)
        {
            unchecked 
            {
                uint imm11_5 = (inst >> 25) & 0x7F;
                uint imm4_0 = (inst >> 7) & 0x1F;
                int imm12 = (int)((imm11_5 << 5) | imm4_0);
                
                // Sign Extend 12-bit to 32-bit manually
                if ((imm12 & 0x800) != 0) 
                    imm12 |= (int)0xFFFFF000;
                    
                return imm12;
            }
        }

        public static int ExtractBTypeImm(uint inst)
        {
            unchecked 
            {
                uint imm12 = (inst >> 31) & 1;
                uint imm10_5 = (inst >> 25) & 0x3F;
                uint imm4_1 = (inst >> 8) & 0xF;
                uint imm11 = (inst >> 7) & 1;
                
                // Reconstruct the 13-bit offset
                int offset = (int)((imm12 << 12) | (imm11 << 11) | (imm10_5 << 5) | (imm4_1 << 1));
                
                // Sign Extend from bit 12
                if ((offset & 0x1000) != 0) 
                    offset |= (int)0xFFFFE000;
                    
                return offset;
            }
        }
    }
}
