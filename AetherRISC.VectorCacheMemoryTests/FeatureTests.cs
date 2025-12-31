using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
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
            
            state.VRegisters.UpdateVtype(0, 4); // SEW=8, LMUL=1, AVL=4

            var data1 = new byte[16]; Array.Fill(data1, (byte)10);
            var data2 = new byte[16]; Array.Fill(data2, (byte)20);
            state.VRegisters.WriteRaw(1, data1);
            state.VRegisters.WriteRaw(2, data2);

            var inst = new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V.VaddVvInstruction(3, 1, 2);
            var d = new AetherRISC.Core.Architecture.Hardware.ISA.InstructionData { Rd=3, Rs1=1, Rs2=2 };
            inst.Execute(state, d);

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
                DramLatencyCycles = 50
            };
            
            cfg.L1D.LatencyCycles = 2;
            cfg.L2.Enabled = false;

            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            var ram = new PhysicalRam(0, 1024);
            state.AttachMemory(ram); 
            
            var cpu = new PipelineController(state, cfg); 
            var bus = (AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy.CachedMemoryBus)state.Memory!;

            // First access: Cold Miss (L1 Latency + DRAM Latency)
            int latency1 = bus.GetAccessLatency(0x100, false); 
            Assert.True(latency1 >= 52); 

            // Second access: Hit (L1 Latency only)
            int latency2 = bus.GetAccessLatency(0x100, false);
            Assert.Equal(2, latency2);
        }
        
        [Fact]
        public void Cache_Size_By_Geometry()
        {
             var cfg = new AetherRISC.Core.Architecture.CacheConfiguration();
             // 512 lines, 16 words/line, 4 bytes/word = 64 bytes/line. 
             // Total = 512 * 64 = 32768 bytes.
             cfg.ConfigureGeometry(512, 16);
             
             Assert.Equal(64, cfg.LineSizeBytes);
             Assert.Equal(32768, cfg.SizeBytes);
        }
    }
}
