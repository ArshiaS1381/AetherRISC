using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Registers;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Simulation.State;

public class MachineState
{
    public SystemConfig Config { get; }
    
    // Register Files
    public RegisterFile Registers { get; }
    public FloatingPointRegisters FRegisters { get; }
    public CsrFile Csr { get; }

    // Memory & Execution State
    public IMemoryBus? Memory { get; set; } 
    public ulong ProgramCounter { get; set; }
    public bool Halted { get; set; } = false;
    
    // Atomics (LR/SC)
    public ulong? LoadReservationAddress { get; set; } 

    // Environment
    public IHostSystem? Host { get; set; }

    public MachineState(SystemConfig config)
    {
        Config = config;
        
        // Initialize Hardware Components
        Registers = new RegisterFile(); 
        FRegisters = new FloatingPointRegisters();
        Csr = new CsrFile();
        
        // Set Initial PC
        ProgramCounter = config.ResetVector;
        LoadReservationAddress = null;
    }
}
