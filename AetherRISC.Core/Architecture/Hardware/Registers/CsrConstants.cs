namespace AetherRISC.Core.Architecture.Hardware.Registers
{
    public static class CsrConstants
    {
        // Machine Trap Setup
        public const uint mstatus = 0x300;
        public const uint misa    = 0x301;
        public const uint mie     = 0x304;
        public const uint mtvec   = 0x305;

        // Machine Trap Handling
        public const uint mscratch = 0x340;
        public const uint mepc     = 0x341;
        public const uint mcause   = 0x342;
        public const uint mtval    = 0x343;
        public const uint mip      = 0x344;
        
        // Cycle Counters
        public const uint cycle    = 0xC00;
        public const uint time     = 0xC01;
        public const uint instret  = 0xC02;
    }
}
