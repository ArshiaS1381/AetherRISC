using Xunit;
using AetherRISC.SuperScalarTests;

namespace AetherRISC.SuperScalarTests
{
    public class ShadowExecutionTests
    {
        [Fact]
        public void Shadows_DoNotWriteRegister_OnMispredict()
        {
            var code = @"
                addi x1, x0, 1
                beq x1, x1, target
                addi x5, x0, 999
                target:
                addi x6, x0, 777
            ";
            var (runner, state) = TestHelper.Setup(code, 4);
            runner.Run(20);
            
            Assert.Equal(0ul, state.Registers[5]); 
            Assert.Equal(777ul, state.Registers[6]);
        }

        [Fact]
        public void Ebreak_Stops_Bundle_Execution()
        {
            var code = @"
                ebreak
                addi x5, x0, 1
            ";
            var (runner, state) = TestHelper.Setup(code, 4);
            runner.Run(10);
            
            // Should be 0. If 1, the shadow instruction executed.
            Assert.Equal(0ul, state.Registers[5]);
            Assert.True(state.Halted);
        }
    }
}
