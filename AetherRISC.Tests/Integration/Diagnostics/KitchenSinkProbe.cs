using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Integration.Diagnostics
{
    public class KitchenSinkProbe : AetherRISC.Tests.Infrastructure.PipelineTestFixture
    {
        private readonly ITestOutputHelper _output;
        public KitchenSinkProbe(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Run_Trace()
        {
            // Replicating Pass 2: [3, 1] -> Should swap to [1, 3]
            var source = @"
                .text
                li x7, 0x200
                li x11, 3
                li x12, 1
                sw x11, 0(x7)
                sw x12, 4(x7)
                
                # Trace Start
                li x9, 5        
                lw x8, 0(x7)    # Load 3
                lw x9, 4(x7)    # Load 1
                
                ble x8, x9, skip
                
                # Fallthrough (Swap)
                sw x9, 0(x7)
                sw x8, 4(x7)
                li x3, 1
                j end
                
                skip:
                li x3, 0
                
                end:
                ebreak
            ";
            
            InitPipeline(1);
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            Machine.Registers.PC = asm.TextBase;

            // Run with tracing
            for(int i=0; i<30; i++)
            {
                Pipeline.Step(i); // Step using Runner directly
                
                // Superscalar Update: Iterate Slots
                var ex = Pipeline.PipelineState.ExecuteMemory;
                
                for(int s=0; s < ex.Slots.Length; s++)
                {
                    var slot = ex.Slots[s];
                    if (slot.Valid && slot.DecodedInst != null && slot.DecodedInst.IsBranch)
                    {
                         _output.WriteLine($"[Cycle {i}] Branch {slot.DecodedInst.Mnemonic} Taken={slot.BranchTaken}");
                    }
                }
                
                if (i == 20)
                {
                    _output.WriteLine($"x8: {Machine.Registers.Read(8)} (Exp 3)");
                    _output.WriteLine($"x9: {Machine.Registers.Read(9)} (Exp 1)");
                }
            }
            
            ulong val0 = Machine.Memory.ReadWord(0x200);
            _output.WriteLine($"[0]: {val0} (Exp 1)");
            
            Assert.Equal(1ul, val0);
        }
    }
}
