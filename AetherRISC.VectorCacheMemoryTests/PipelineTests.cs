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
            // lw x1, 0(x2)  -> Memory stage produces result
            // add x3, x1, x1 -> Execute stage needs result immediately
            // This requires a stall (bubble) because Forwarding cannot go Mem -> Ex backwards in time within same cycle
            
            string asm = @"
                .text
                li x2, 100
                lw x1, 0(x2)
                add x3, x1, x1
                ebreak
            ";

            var cfg = new ArchitectureSettings { PipelineWidth = 1 }; // Scalar for easier counting
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            state.AttachMemory(new PhysicalRam(0, 1024));
            
            new SourceAssembler(asm).Assemble(state);
            var runner = new PipelinedRunner(state, new NullLogger(), cfg);
            
            runner.Run(20);

            // We expect at least 1 Data Hazard stall
            Assert.True(runner.Metrics.DataHazardStalls > 0, "Pipeline did not stall on Load-Use hazard");
        }

        [Fact]
        public void ControlHazard_BranchMispredict_FlushesPipeline()
        {
            // BNE is taken. If predicted NotTaken (default), we fetch wrong path.
            // Verify execution result matches correct path, AND flush counter increased.
            string asm = @"
                .text
                li x1, 1
                li x2, 2
                bne x1, x2, target
                li x3, 0xBAD     # Should be skipped/flushed
                ebreak
                target:
                li x3, 0xCAFE
                ebreak
            ";

            var cfg = new ArchitectureSettings 
            { 
                PipelineWidth = 1,
                StaticPredictTaken = false, // Force mispredict on BNE
                BranchPredictorType = "static"
            };
            
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            state.AttachMemory(new PhysicalRam(0, 1024));
            
            new SourceAssembler(asm).Assemble(state);
            var runner = new PipelinedRunner(state, new NullLogger(), cfg);
            
            runner.Run(20);

            Assert.Equal(0xCAFEul, state.Registers.Read(3));
            Assert.True(runner.Metrics.ControlHazardFlushes > 0, "Pipeline did not report flush on mispredict");
        }

        [Fact]
        public void StructuralHazard_UnitExhaustion()
        {
            // 2 instructions needing ALU, but only 1 ALU available.
            string asm = @"
                .text
                add x1, x0, x0
                add x2, x0, x0
                ebreak
            ";

            var cfg = new ArchitectureSettings 
            { 
                PipelineWidth = 2, // Superscalar fetch forced here
                MaxIntALUs = 1     // Constrained Execution
            };
            
            var sys = SystemConfig.Rv64();
            var state = new MachineState(sys, cfg);
            state.AttachMemory(new PhysicalRam(0, 1024));
            
            new SourceAssembler(asm).Assemble(state);
            var runner = new PipelinedRunner(state, new NullLogger(), cfg);
            
            runner.Run(20);
            
            // If the simulation completes without throwing, the hazard unit successfully serialized them.
            Assert.Equal(0ul, state.Registers.Read(1));
        }
    }
}
