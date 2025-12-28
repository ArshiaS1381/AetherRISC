using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory; // SystemBus
using AetherRISC.Core.Architecture.Hardware.Pipeline; // PipelineController
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Tests.Integration; // TestAssembler
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

namespace AetherRISC.Core.Tests
{
    public class MinimalTest
    {
        [Fact]
        public void Straight_Line_Code_Must_Execute()
        {
            // 1. Setup Machine
            var state = new MachineState(SystemConfig.Rv64());
            state.Memory = new SystemBus(1024);
            var pipeline = new PipelineController(state);
            var asm = new TestAssembler();
            state.ProgramCounter = 0;

            // 2. Program: ADDI x1, x0, 10
            asm.Add(pc => Inst.Addi(1, 0, 10));

            // Load into Memory
            var insts = asm.Assemble();
            for(int i=0; i < insts.Count; i++) 
                state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

            // 3. Run Pipeline (Fetch, Decode, Execute, Memory, Writeback)
            // It takes ~5 cycles for the first instruction to fully retire, 
            // but the Register Write happens in Execute (Cycle 3).
            for(int i=0; i<5; i++) pipeline.Cycle();

            // 4. Verify Result
            // Check that x1 holds the value 10
            Assert.Equal((ulong)10, state.Registers.Read(1));
            
            // Note: We removed Assert.Single() because checking the specific "event list" 
            // of the old emulator is no longer relevant. We care about the ARCHITECTURAL STATE (Registers).
        }
    }
}




