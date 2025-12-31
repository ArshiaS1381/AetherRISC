using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Memory.Physical;
using System;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class VectorTests
    {
        [Fact]
        public void VSETVLI_Calculates_VL_Correctly()
        {
            var cfg = new ArchitectureSettings { EnableVectors = true, VectorLenBits = 128 };
            var state = new MachineState(SystemConfig.Rv64(), cfg);

            // SEW 8 (1 byte). 128 bits = 16 bytes. Max VL = 16.
            // Request 20.
            state.VRegisters.UpdateVtype(0, 20); // 0 = e8, m1
            
            Assert.Equal(16, state.VRegisters.Vl);
            Assert.Equal(1, state.VRegisters.SewBytes);
        }

        [Fact]
        public void VADD_VV_AddsElements()
        {
            var cfg = new ArchitectureSettings { EnableVectors = true, VectorLenBits = 128 };
            var state = new MachineState(SystemConfig.Rv64(), cfg);
            state.AttachMemory(new PhysicalRam(0, 1024));

            state.VRegisters.UpdateVtype(0, 4); // 4 elements

            var a = new byte[16]; a[0]=1; a[1]=2;
            var b = new byte[16]; b[0]=10; b[1]=20;
            
            state.VRegisters.WriteRaw(1, a);
            state.VRegisters.WriteRaw(2, b);

            var inst = new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V.VaddVvInstruction(3, 1, 2);
            var d = new AetherRISC.Core.Architecture.Hardware.ISA.InstructionData { Rd=3, Rs1=1, Rs2=2 };
            inst.Execute(state, d);

            var res = state.VRegisters.GetRaw(3);
            Assert.Equal(11, res[0]);
            Assert.Equal(22, res[1]);
            Assert.Equal(0, res[2]); // Tail should be undisturbed/zero (simplified)
        }
    }
}
