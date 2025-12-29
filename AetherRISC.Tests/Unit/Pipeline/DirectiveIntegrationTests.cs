using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class DirectiveIntegrationTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public DirectiveIntegrationTests(ITestOutputHelper output) => _output = output;

    private void AssembleAndLoad(string source)
    {
        InitPipeline();
        var assembler = new SourceAssembler(source);
        assembler.Assemble(Machine);
        Machine.Registers.PC = assembler.TextBase;
    }

    [Fact]
    public void Data_And_Text_Sections_Interaction()
    {
        // Use LWU to load 32-bit unsigned values in RV64
        var source = @"
            .data
            var1: .word 0xDEADBEEF
            var2: .word 0xCAFEBABE
            
            .text
            .globl _start
            _start:
                la   x1, var1
                lwu  x2, 0(x1)      # Load 0xDEADBEEF unsigned
                
                la   x3, var2
                lwu  x4, 0(x3)      # Load 0xCAFEBABE unsigned
                
                add  x5, x2, x4     
                ebreak
        ";

        AssembleAndLoad(source);
        Cycle(50);
        
        Assert.True(Machine.Halted, "Machine did not halt.");
        AssertReg(2, 0xDEADBEEFul);
        AssertReg(4, 0xCAFEBABEul);
        
        ulong sum = 0xDEADBEEFul + 0xCAFEBABEul;
        AssertReg(5, sum);
    }

    [Fact]
    public void Align_Directive_Correctness()
    {
        var source = @"
            .data
            b1: .byte 0x11
            
            .align 2
            w1: .word 0x22222222
            
            .align 3
            d1: .word 0x33333333
            
            .text
            la x1, b1
            la x2, w1
            la x3, d1
            ebreak
        ";

        AssembleAndLoad(source);
        Cycle(20);

        ulong addrB1 = Machine.Registers.Read(1);
        ulong addrW1 = Machine.Registers.Read(2);
        ulong addrD1 = Machine.Registers.Read(3);

        _output.WriteLine($"b1: {addrB1:X}");
        _output.WriteLine($"w1: {addrW1:X}");
        _output.WriteLine($"d1: {addrD1:X}");

        Assert.Equal(addrB1 + 4, addrW1);
        Assert.Equal(addrW1 + 4, addrD1);
        
        Assert.Equal(0x11u, Machine.Memory.ReadByte((uint)addrB1));
        Assert.Equal(0x22222222u, Machine.Memory.ReadWord((uint)addrW1));
    }

    [Fact]
    public void Space_Directive_Reservation()
    {
        var source = @"
            .data
            start:  .word 0xFFFFFFFF
            .space  12
            end:    .word 0xEEEEEEEE
            
            .text
            la x1, start
            la x2, end
            sub x3, x2, x1
            ebreak
        ";

        AssembleAndLoad(source);
        Cycle(20);

        AssertReg(3, 16ul);
        ulong startAddr = Machine.Registers.Read(1);
        Assert.Equal(0u, Machine.Memory.ReadWord((uint)startAddr + 4));
        Assert.Equal(0u, Machine.Memory.ReadWord((uint)startAddr + 8));
        Assert.Equal(0u, Machine.Memory.ReadWord((uint)startAddr + 12));
    }

    [Fact]
    public void BSS_Section_Zero_Initialization()
    {
        // Use LWU to avoid sign extension
        var source = @"
            .bss
            var_bss: .space 4
            
            .data
            var_data: .word 0xAAAA5555
            
            .text
            la x1, var_bss
            lw x2, 0(x1)
            la x3, var_data
            lwu x4, 0(x3)     # Load Unsigned
            ebreak
        ";

        AssembleAndLoad(source);
        Cycle(20);

        AssertReg(2, 0ul);
        AssertReg(4, 0xAAAA5555ul);
    }
}
