namespace AetherRISC.Core.Architecture
{
    public struct InstructionData
    {
        public int Rd { get; set; }
        public int Rs1 { get; set; }
        public int Rs2 { get; set; }
        public ulong Immediate { get; set; }
        public uint Raw { get; set; }
        public ulong PC { get; set; }

        public InstructionData(uint raw, ulong pc)
        {
            Raw = raw;
            PC = pc;
            
            Rd = (int)((raw >> 7) & 0x1F);
            Rs1 = (int)((raw >> 15) & 0x1F);
            Rs2 = (int)((raw >> 20) & 0x1F);
            
            // Default Immediate extraction
            int imm = (int)(raw >> 20);
            if ((imm & 0x800) != 0) imm |= unchecked((int)0xFFFFF000);
            Immediate = (ulong)(long)imm;
        }
    }
}
