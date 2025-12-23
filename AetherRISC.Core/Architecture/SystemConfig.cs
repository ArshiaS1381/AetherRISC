namespace AetherRISC.Core.Architecture
{
    public class SystemConfig
    {
        public int XLEN { get; set; } = 64; // 32 or 64

        public static SystemConfig Rv64() => new SystemConfig { XLEN = 64 };
        public static SystemConfig Rv32() => new SystemConfig { XLEN = 32 };
    }
}
