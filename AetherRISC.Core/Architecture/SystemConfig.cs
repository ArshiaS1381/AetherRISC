namespace AetherRISC.Core.Architecture;

public enum ArchitectureMode { Rv32, Rv64 }

public class SystemConfig
{
    public ArchitectureMode Architecture { get; set; }
    public bool EnableFloats { get; set; }
    public bool EnableVectors { get; set; }
    public int VectorLengthBits { get; set; }

    public static SystemConfig Rv64() => new SystemConfig 
    { 
        Architecture = ArchitectureMode.Rv64, 
        EnableFloats = true 
    };

    public static SystemConfig Rv32() => new SystemConfig 
    { 
        Architecture = ArchitectureMode.Rv32, 
        EnableFloats = true 
    };
}
