namespace AetherRISC.Core.Architecture.Simulation.State;

public class SystemConfig
{
    public int XLEN { get; }
    public ulong ResetVector { get; }

    public SystemConfig(int xlen, ulong resetVector = 0x80000000)
    {
        XLEN = xlen;
        ResetVector = resetVector;
    }

    public static SystemConfig Rv32() => new SystemConfig(32);
    public static SystemConfig Rv64() => new SystemConfig(64);
}
