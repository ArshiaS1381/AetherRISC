using AetherRISC.Core.Architecture.Registers;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture;

public class MachineState
{
    public SystemConfig Config { get; }
    public ulong ProgramCounter { get; set; }

    public RegisterFile Registers { get; }
    public ControlStatusRegisters Csr { get; }
    public IMemoryBus? Memory { get; set; }
    public ISystemCallHandler? Host { get; set; }

    public FloatingPointRegisters? FloatRegisters { get; }
    public VectorRegisters? VectorRegisters { get; }

    public MachineState(SystemConfig config)
    {
        Config = config;
        // Pass config to RegisterFile so it knows whether to clamp
        Registers = new RegisterFile(config);
        Csr = new ControlStatusRegisters();

        if (config.EnableFloats) FloatRegisters = new FloatingPointRegisters();
        if (config.EnableVectors) VectorRegisters = new VectorRegisters(config.VectorLengthBits);
    }
}
