using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Integration.Diagnostics
{
    // Explicitly inherit from Infrastructure.PipelineTestFixture to resolve ambiguity
    public class StallPersistenceProbe : AetherRISC.Tests.Infrastructure.PipelineTestFixture
    {
        private readonly ITestOutputHelper _output;
        public StallPersistenceProbe(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Probe_Stall_Persistence_In_Memory_Stage()
        {
            var source = @"
                .text
                li x10, 0x100
                lw x1, 0(x10)
                nop
                addi x2, x1, 0
                ebreak
            ";
            
            InitPipeline(1); // 1-wide
            Machine.Memory.WriteWord(0x100, 0xAAAAAAAA);
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            Machine.Registers.PC = asm.TextBase;

            Cycle(4); 
            Cycle(1); 
            
            // FetchDecode buffer itself has the Stall flag
            var ifId = PipelineBuffers.FetchDecode;
            
            _output.WriteLine($"Is Fetch Stalled? {ifId.IsStalled}");
            Assert.True(ifId.IsStalled, "Pipeline failed to stall when Load was in Memory Stage.");
            
            Cycle(5);
            ulong x2 = Machine.Registers.Read(2);
            
            // LW sign extends
            Assert.Equal(0xFFFFFFFFAAAAAAAAul, x2);
        }
    }
}
