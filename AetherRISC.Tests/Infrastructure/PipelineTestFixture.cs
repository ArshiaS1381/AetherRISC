using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

namespace AetherRISC.Tests.Infrastructure
{
    public class PipelineTestFixture : CpuTestFixture
    {
        public PipelinedRunner Pipeline => (PipelinedRunner)Runner;
        public PipelineBuffers PipelineBuffers => Pipeline.PipelineState;

        // --- Helpers for legacy 1-wide tests ---
        public PipelineMicroOp FetchDecodeSlot => PipelineBuffers.FetchDecode.Slots[0];
        public PipelineMicroOp DecodeExecuteSlot => PipelineBuffers.DecodeExecute.Slots[0];
        public PipelineMicroOp ExecuteMemorySlot => PipelineBuffers.ExecuteMemory.Slots[0];
        public PipelineMicroOp MemoryWritebackSlot => PipelineBuffers.MemoryWriteback.Slots[0];

        public void InitPipeline(int width = 1)
        {
            var config = new SystemConfig(64, resetVector: 0x0000);
            Machine = new MachineState(config);
            
            Memory = new TestMemoryBus(1024 * 1024); 
            Machine.Memory = Memory;

            Assembler = new Core.Assembler.TestAssembler();
            
            var settings = new ArchitectureSettings 
            { 
                PipelineWidth = width,
                EnableEarlyBranchResolution = true,
                BranchPredictorInitialValue = 1
            };

            Runner = new PipelinedRunner(Machine, new NullLogger(), "static", settings);
        }

        // Alias for tests using the old name
        public void InitSuperscalar(int width) => InitPipeline(width);

        public void Cycle(int count)
        {
            for (int i = 0; i < count; i++) Pipeline.Step(i);
        }

        // Overload for default 1 cycle
        public void Cycle() => Cycle(1);

        public void LoadProgram()
        {
            var insts = Assembler.Assemble();
            uint addr = (uint)Machine.Config.ResetVector;
            foreach (var inst in insts)
            {
                uint raw = InstructionEncoder.Encode(inst);
                Memory.WriteWord(addr, raw);
                addr += 4;
            }
            Machine.Registers.PC = Machine.Config.ResetVector;
        }
    }
}
