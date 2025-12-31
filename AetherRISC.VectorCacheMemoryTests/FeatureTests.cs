using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Memory.Physical;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using System;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class FeatureTests
    {
        [Fact]
        public void VectorAdd_CalculatesCorrectly()
        {
            var cfg = new ArchitectureSettings { EnableVectors = true, VectorLenBits = 128 };
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            var ram = new PhysicalRam(0, 1024);
            state.AttachMemory(ram);
            
            // Setup VType and VL
            state.VRegisters.UpdateVtype(0, 4); // SEW=8, LMUL=1, AVL=4

            // Load Data into V1 and V2
            var data1 = new byte[16]; Array.Fill(data1, (byte)10);
            var data2 = new byte[16]; Array.Fill(data2, (byte)20);
            state.VRegisters.WriteRaw(1, data1);
            state.VRegisters.WriteRaw(2, data2);

            // Execute VADD.VV
            var inst = new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V.VaddVvInstruction(3, 1, 2);
            var d = new AetherRISC.Core.Architecture.Hardware.ISA.InstructionData { Rd=3, Rs1=1, Rs2=2 };
            inst.Execute(state, d);

            // Verify
            var res = state.VRegisters.GetRaw(3);
            Assert.Equal(30, res[0]); 
            Assert.Equal(30, res[3]); 
            Assert.Equal(0, res[4]); 
        }

        [Fact]
        public void Cache_ReportsMissLatencies()
        {
            var cfg = new ArchitectureSettings 
            { 
                EnableCacheSimulation = true,
                L1DCacheLatency = 2,
                EnableL2Cache = false,
                DramLatencyCycles = 50
            };
            
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            var ram = new PhysicalRam(0, 1024);
            state.AttachMemory(ram); 
            
            var cpu = new PipelineController(state, cfg); 
            var bus = (AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy.CachedMemoryBus)state.Memory!;

            // First access: Cold Miss
            int latency1 = bus.GetAccessLatency(0x100, false); 
            Assert.True(latency1 >= 52); 

            // Second access: Hit
            int latency2 = bus.GetAccessLatency(0x100, false);
            Assert.Equal(2, latency2);
        }
    }
}
