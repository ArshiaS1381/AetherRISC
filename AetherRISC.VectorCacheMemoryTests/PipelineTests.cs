using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Memory.Physical;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Helpers;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class PipelineTests
    {
        [Fact]
        public void DataHazard_LoadUse_CreatesStall()
        {
            string asm = @"
                .text
                li x2, 100
                lw x1, 0(x2)
                add x3, x1, x1
                ebreak
            ";

            var cfg = new ArchitectureSettings { PipelineWidth = 1 };
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            var ram = new PhysicalRam(0, 1024);
            state.AttachMemory(ram);
            
            new SourceAssembler(asm).Assemble(state);
            var runner = new PipelinedRunner(state, new NullLogger(), cfg);
            
            runner.Run(20);

            Assert.True(runner.Metrics.DataHazardStalls > 0, "Pipeline did not stall");
            
            state.Memory?.WriteWord(100, 10);
            
            state.ProgramCounter = sys.ResetVector;
            state.Halted = false;
            runner = new PipelinedRunner(state, new NullLogger(), cfg);
            
            runner.Run(50);
            
            Assert.Equal(20ul, state.Registers.Read(3)); 
        }

        [Fact]
        public void ControlHazard_BranchMispredict_FlushesPipeline()
        {
            string asm = @"
                .text
                li x1, 1
                li x2, 2
                bne x1, x2, target
                li x3, 0xBAD     
                ebreak
                target:
                li x3, 0xCAFE
                ebreak
            ";

            var cfg = new ArchitectureSettings 
            { 
                PipelineWidth = 1,
                StaticPredictTaken = false,
                BranchPredictorType = "static"
            };
            
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            state.AttachMemory(new PhysicalRam(0, 1024));
            
            new SourceAssembler(asm).Assemble(state);
            var runner = new PipelinedRunner(state, new NullLogger(), cfg);
            
            runner.Run(50); 

            Assert.Equal(0xCAFEul, state.Registers.Read(3));
            Assert.True(runner.Metrics.ControlHazardFlushes > 0, "Flush count 0");
        }

        [Fact]
        public void StructuralHazard_UnitExhaustion()
        {
            string asm = @"
                .text
                add x1, x0, x0
                add x2, x0, x0
                ebreak
            ";

            var cfg = new ArchitectureSettings 
            { 
                PipelineWidth = 2,
                MaxIntALUs = 1
            };
            
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            state.AttachMemory(new PhysicalRam(0, 1024));
            
            new SourceAssembler(asm).Assemble(state);
            var runner = new PipelinedRunner(state, new NullLogger(), cfg);
            
            runner.Run(20);
            
            Assert.Equal(0ul, state.Registers.Read(1));
        }
    }
}
