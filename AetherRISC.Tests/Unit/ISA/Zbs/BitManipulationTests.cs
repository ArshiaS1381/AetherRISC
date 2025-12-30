using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.Zbs
{
    public class BitManipulationTests : AetherRISC.Tests.Infrastructure.PipelineTestFixture
    {
        [Fact]
        public void Bclr_ClearsBit()
        {
            InitPipeline(1);
            Machine.Registers.Write(1, 0b1111);
            Machine.Registers.Write(2, 1); // Clear bit 1

            Assembler.Add(pc => new AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbs.BclrInstruction(3, 1, 2));
            LoadProgram();
            Cycle(5);

            AssertReg(3, 0b1101);
        }
        
        // Assuming other tests exist, the key is the class inheritance fix above.
        // If you had more logic here, ensure it inherits correctly.
    }
}
