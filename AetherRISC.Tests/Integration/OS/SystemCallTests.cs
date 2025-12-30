using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Integration.OS
{
    public class SystemCallTests : CpuTestFixture
    {
        private readonly ITestOutputHelper _output;
        public SystemCallTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Ecall_Exit_Terminates()
        {
            Init64();
            Assembler.Add(pc => Inst.Addi(17, 0, 93)); // Exit
            Assembler.Add(pc => Inst.Addi(10, 0, 0));  // Code 0
            Assembler.Add(pc => Inst.Ecall(0, 0, 0));
            
            base.Run(10);
            
            Assert.True(Machine.Halted);
        }

        [Fact]
        public void Ecall_PrintInt()
        {
            Init64();
            Assembler.Add(pc => Inst.Addi(17, 0, 1)); // Print Int
            Assembler.Add(pc => Inst.Addi(10, 0, 42)); 
            Assembler.Add(pc => Inst.Ecall(0, 0, 0));
            Assembler.Add(pc => Inst.Addi(17, 0, 93)); // Exit
            Assembler.Add(pc => Inst.Ecall(0, 0, 0));

            base.Run(20);
            
            Assert.True(Machine.Halted);
        }
    }
}
