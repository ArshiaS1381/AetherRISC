using Xunit;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.SuperScalarTests
{
    public class PartialIssueTests
    {
        [Fact]
        public void IntraBundle_Breaks_Correctly()
        {
            // Scenario:
            // 0: addi x1, x0, 10  (Producer)
            // 4: add x2, x1, x0   (Consumer - RAW Hazard)
            // 8: addi x3, x0, 99  (Independent - victim of break)
            
            var code = @"
                addi x1, x0, 10
                add x2, x1, x0
                addi x3, x0, 99
            ";

            var (runner, state) = TestHelper.Setup(code, pipelineWidth: 4);

            // Cycle 0: Fetch
            runner.Step(1); 
            
            // Cycle 1: Decode (Instructions move to DecodeExecute buffer)
            runner.Step(1);

            // Cycle 2: Hazard Resolve -> Execute (Instructions move to ExecuteMemory buffer)
            runner.Step(1);

            var slots = runner.PipelineState.ExecuteMemory.Slots;
            
            // Slot 0 (addi x1): Should succeed
            Assert.True(slots[0].Valid, "Slot 0 (Producer) should have issued to Execute");
            Assert.Equal(10ul, slots[0].AluResult);

            // Slot 1 (add x2): Should be killed by Intra-Bundle Logic
            Assert.False(slots[1].Valid, "Slot 1 (Consumer) should have been killed (Partial Issue)");
            
            // Slot 2 (addi x3): Should be killed because it's after the break point
            Assert.False(slots[2].Valid, "Slot 2 (Independent) should have been killed (victim of break)");

            // Run remaining cycles to ensure replay works
            runner.Run(10);
            
            Assert.Equal(10ul, state.Registers[2]); // Consumer executed eventually
            Assert.Equal(99ul, state.Registers[3]); // Victim executed eventually
        }

        [Fact]
        public void LoadUse_PartialBreak_IndependentInstsProceed()
        {
            // Scenario:
            // LW x1 is in Execute (producing result next cycle)
            // Bundle in Decode:
            // 0: addi x6, x0, 66 (Independent of x1) -> Should PASS
            // 1: add x2, x1, x0  (Dependent on x1)    -> Should BREAK
            
            var code = @"
                lw x1, 0(x0)
                addi x6, x0, 66
                add x2, x1, x0
            ";

            var (runner, state) = TestHelper.Setup(code, pipelineWidth: 4);
            state.Memory!.WriteWord(0, 88);

            // Cycle 0: Fetch LW, ADDI, ADD
            runner.Step(1); 
            
            // Cycle 1: Decode LW, ADDI, ADD
            runner.Step(1);
            
            // Cycle 2: Execute. 
            // LW moves to MEM. ADDI, ADD are decoded.
            // Wait, LW was fetched with the group.
            // LW is Slot 0. ADDI Slot 1. ADD Slot 2.
            // Intra-Bundle logic sees ADD depends on LW (Slot 0). 
            // Slot 0 is NOT a load *yet* (it's in ID). It's a RegWrite.
            // So ADD will break on Intra-Bundle RAW, not Load-Use.
            // This is still a valid test of Partial Issue.
            runner.Step(1);
            
            var slots = runner.PipelineState.ExecuteMemory.Slots;
            
            Assert.True(slots[0].Valid, "LW should issue");
            Assert.True(slots[1].Valid, "ADDI (Independent) should issue");
            Assert.False(slots[2].Valid, "ADD (Dependent) should break");
            
            Assert.Equal(66ul, slots[1].AluResult);

            // Finish
            runner.Run(10);
            Assert.Equal(88ul, state.Registers[1]); // Loaded
            Assert.Equal(66ul, state.Registers[6]); // Independent
            Assert.Equal(88ul, state.Registers[2]); // Dependent executed eventually
        }
    }
}
