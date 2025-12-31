using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Helpers;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class SystemTests
    {
        [Fact]
        public void Pipeline_DataHazard_Stall()
        {
            var cfg = new ArchitectureSettings { PipelineWidth = 1 };
            var s = new MachineState(SystemConfig.Rv64(), cfg);
            s.AttachMemory(new SystemBus(0xFFFFFFFF));
            
            new SourceAssembler("li x2, 100\nlw x1, 0(x2)\nadd x3, x1, x1\nebreak").Assemble(s);
            
            var r = new PipelinedRunner(s, new NullLogger(), cfg);
            r.Run(20);
            
            Assert.True(r.Metrics.DataHazardStalls > 0);
        }

        [Fact]
        public void Pipeline_ControlHazard_Flush()
        {
            var cfg = new ArchitectureSettings { PipelineWidth = 1, StaticPredictTaken = false };
            var s = new MachineState(SystemConfig.Rv64(), cfg);
            s.AttachMemory(new SystemBus(0xFFFFFFFF));
            
            new SourceAssembler("li x1,1\nli x2,2\nbne x1,x2,tgt\nnop\nnop\ntgt:\nli x3,99\nebreak").Assemble(s);
            
            var r = new PipelinedRunner(s, new NullLogger(), cfg);
            r.Run(50);
            
            Assert.Equal(99ul, s.Registers.Read(3));
            Assert.True(r.Metrics.ControlHazardFlushes > 0);
        }

        [Fact]
        public void StructuralHazard_UnitExhaustion()
        {
            var cfg = new ArchitectureSettings { PipelineWidth = 2, MaxIntALUs = 1 };
            var s = new MachineState(SystemConfig.Rv64(), cfg);
            s.AttachMemory(new SystemBus(0xFFFFFFFF));
            
            new SourceAssembler("add x1,x0,x0\nadd x2,x0,x0\nebreak").Assemble(s);
            
            var r = new PipelinedRunner(s, new NullLogger(), cfg);
            r.Run(10);
            
            Assert.Equal(0ul, s.Registers.Read(1));
        }

        [Fact]
        public void Vector_Basic()
        {
            var cfg = new ArchitectureSettings { EnableVectors = true };
            var s = new MachineState(SystemConfig.Rv64(), cfg);
            s.AttachMemory(new SystemBus(0xFFFFFFFF));
            
            s.VRegisters.UpdateVtype(0, 4);
            s.VRegisters.WriteRaw(1, new byte[]{1,1,1,1});
            s.VRegisters.WriteRaw(2, new byte[]{2,2,2,2});
            
            var inst = new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V.VaddVvInstruction(3, 1, 2);
            var d = new AetherRISC.Core.Architecture.Hardware.ISA.InstructionData { Rd=3, Rs1=1, Rs2=2 };
            inst.Execute(s, d);
            
            var res = s.VRegisters.GetRaw(3);
            Assert.Equal(3, res[0]);
        }
    }
}
