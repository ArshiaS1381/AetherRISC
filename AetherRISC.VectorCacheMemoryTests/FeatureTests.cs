using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Memory.Physical;
using System;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class FeatureTests
    {
        [Fact]
        public void Vector_VADD_VV_8bit()
        {
            var cfg = new ArchitectureSettings { EnableVectors = true, VectorLenBits = 128 };
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            var ram = new PhysicalRam(0, 1024);
            state.AttachMemory(ram);
            
            // Set VTYPE: SEW=8, LMUL=1, AVL=16
            state.VRegisters.UpdateVtype(0, 16); 

            // Setup Data
            var v1 = new byte[16]; Array.Fill(v1, (byte)5);
            var v2 = new byte[16]; Array.Fill(v2, (byte)10);
            state.VRegisters.WriteRaw(1, v1);
            state.VRegisters.WriteRaw(2, v2);

            // VADD.VV v3, v1, v2
            var inst = new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V.VaddVvInstruction(3, 1, 2);
            var d = new AetherRISC.Core.Architecture.Hardware.ISA.InstructionData { Rd=3, Rs1=1, Rs2=2 };
            inst.Execute(state, d);

            var res = state.VRegisters.GetRaw(3);
            for(int i=0; i<16; i++) Assert.Equal(15, res[i]);
        }

        [Fact]
        public void Vector_VSETVLI_UpdatesVL()
        {
            var cfg = new ArchitectureSettings { EnableVectors = true, VectorLenBits = 128 };
            var state = new MachineState(SystemConfig.Rv64(), cfg);

            // Requested AVL = 100. 
            // MaxVL for 8-bit SEW, 128-bit VLEN = 16 elements.
            // Result VL should be 16.
            var inst = new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V.VsetvliInstruction(1, 2, 0); 
            state.Registers.Write(2, 100);
            
            var d = new AetherRISC.Core.Architecture.Hardware.ISA.InstructionData { Rd=1, Rs1=2, Imm=0 };
            inst.Execute(state, d);

            Assert.Equal(16ul, state.Registers.Read(1));
            Assert.Equal(16, state.VRegisters.Vl);
        }

        [Fact]
        public void CacheConfig_CalculatesGeometry()
        {
             var cfg = new AetherRISC.Core.Architecture.CacheConfiguration();
             // 512 lines * 16 words/line * 4 bytes/word = 32KB
             cfg.ConfigureGeometry(512, 16);
             Assert.Equal(64, cfg.LineSizeBytes);
             Assert.Equal(32768, cfg.SizeBytes);
        }
    }
}
