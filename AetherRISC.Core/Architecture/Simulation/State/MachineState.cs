using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Registers;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy;
using AetherRISC.Core.Abstractions.Diagnostics;

namespace AetherRISC.Core.Architecture.Simulation.State
{
    public class MachineState
    {
        public ArchitectureSettings Config { get; }
        public SystemConfig SysConfig { get; }
        
        public RegisterFile Registers { get; }
        public FloatingPointRegisters FRegisters { get; }
        public VectorRegisters VRegisters { get; }
        public CsrFile Csr { get; }

        public IMemoryBus? Memory { get; set; } 
        public ulong ProgramCounter { get; set; }
        public bool Halted { get; set; } = false;
        public ulong? LoadReservationAddress { get; set; } 
        public IHostSystem? Host { get; set; }

        public MachineState(SystemConfig sysConfig, ArchitectureSettings archSettings)
        {
            SysConfig = sysConfig;
            Config = archSettings;
            
            Registers = new RegisterFile(); 
            FRegisters = new FloatingPointRegisters();
            VRegisters = new VectorRegisters(archSettings.VectorLenBits);
            Csr = new CsrFile();
            
            ProgramCounter = sysConfig.ResetVector;
        }

        public void AttachMemory(IMemoryBus physicalRam, PerformanceMetrics? metrics = null)
        {
            if (Config.EnableCacheSimulation && metrics != null)
            {
                Memory = new CachedMemoryBus(physicalRam, Config, metrics);
            }
            else
            {
                Memory = physicalRam;
            }
        }
    }
}
