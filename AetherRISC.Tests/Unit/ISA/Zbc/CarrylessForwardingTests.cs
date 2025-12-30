using Xunit;
using AetherRISC.Tests.Infrastructure; // Required for PipelineTestFixture
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc;

namespace AetherRISC.Tests.Unit.ISA.Zbc
{
    public class CarrylessForwardingTests : PipelineTestFixture
    {
        [Fact]
        public void Clmul_Forwards_Result_To_Next_Instruction()
        {
            InitPipeline(1);

            // RAW Hazard Test for Zbc extension
            // x1 = 3
            // x2 = 2
            Machine.Registers.Write(1, 3);
            Machine.Registers.Write(2, 2);

            // 1. CLMUL x3, x1, x2  (3 * 2 carryless = 6)
            // 2. CLMUL x4, x3, x1  (Result x3 used immediately. 6 * 3 carryless)
            
            // Calculation for 6 clmul 3:
            // 6 = 110_2, 3 = 011_2
            // i=0 (rhs bit 1): 110 << 0 = 110
            // i=1 (rhs bit 1): 110 << 1 = 1100
            // 110 ^ 1100 = 1010_2 = 10_10
            
            Assembler.Add(pc => new ClmulInstruction(3, 1, 2));
            Assembler.Add(pc => new ClmulInstruction(4, 3, 1)); // Hazard on x3

            LoadProgram();
            Cycle(10);

            AssertReg(3, 6);
            AssertReg(4, 10);
        }
    }
}
