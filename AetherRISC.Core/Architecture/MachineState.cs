using AetherRISC.Core.Architecture.Registers;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Architecture
{
    public class MachineState
    {
        public RegisterFile Registers { get; } = new RegisterFile();
        
        // UPGRADE: Using your existing class name
        public FloatingPointRegisters FRegisters { get; } = new FloatingPointRegisters(); 
        
        public CsrFile Csr { get; } = new CsrFile();
        
        public ulong ProgramCounter { get; set; }
        public IMemoryBus? Memory { get; set; }
        public SystemConfig Config { get; }
        public dynamic? Host { get; set; } 

        public ulong? LoadReservationAddress { get; set; } = null;

        public MachineState(SystemConfig config)
        {
            Config = config;
        }
    }
}
