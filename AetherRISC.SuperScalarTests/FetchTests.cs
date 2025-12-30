using Xunit;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.SuperScalarTests
{
    public class FetchTests
    {
        [Fact]
        public void Fetch_Stops_At_Predicted_Taken_Branch()
        {
            // Code:
            // 0: nop
            // 4: beq x0, x0, target (Taken)
            // 8: nop (Should NOT be fetched in same cycle)
            // ...
            // target: nop
            
            var code = @"
                nop
                beq x0, x0, target
                nop
                target:
                nop
            ";
            
            var (runner, state) = TestHelper.Setup(code, pipelineWidth: 4);

            // Cycle 1: Fetch
            runner.Step(1);

            // Check Buffer
            var slot0 = runner.PipelineState.FetchDecode.Slots[0];
            var slot1 = runner.PipelineState.FetchDecode.Slots[1];
            var slot2 = runner.PipelineState.FetchDecode.Slots[2];

            Assert.True(slot0.Valid, "Slot 0 (NOP) fetched");
            Assert.True(slot1.Valid, "Slot 1 (BEQ) fetched");
            
            // This is the proof of the bottleneck:
            Assert.False(slot2.Valid, "Slot 2 should be empty because Fetch stopped at Branch");
        }

        [Fact]
        public void Fetch_Continues_Past_NotTaken_Branch()
        {
            // Code:
            // 0: bne x0, x0, target (Not Taken)
            // 4: nop
            // 8: nop
            
            var code = @"
                bne x0, x0, target
                nop
                nop
                target:
            ";
            
            var (runner, state) = TestHelper.Setup(code, pipelineWidth: 4);

            runner.Step(1);

            Assert.True(runner.PipelineState.FetchDecode.Slots[0].Valid, "Slot 0 (BNE) fetched");
            Assert.True(runner.PipelineState.FetchDecode.Slots[1].Valid, "Slot 1 (NOP) fetched - Contiguous fetch works");
            Assert.True(runner.PipelineState.FetchDecode.Slots[2].Valid, "Slot 2 (NOP) fetched");
        }
    }
}
