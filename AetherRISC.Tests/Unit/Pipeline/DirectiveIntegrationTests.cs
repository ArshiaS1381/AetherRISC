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

    /// <summary>
    /// Helper to assemble text-based source code and load it into the machine.
    /// Overrides the default TestAssembler logic used in the base fixture.
    /// </summary>
    private void AssembleAndLoad(string source)
    {
        InitPipeline();
        
        // Use the text-based SourceAssembler
        var assembler = new SourceAssembler(source);
        
        // This populates Machine.Memory directly and builds the symbol table
        assembler.Assemble(Machine);
        
        // SourceAssembler sets PC to TextBase (0x00400000) automatically,
        // but we explicitly ensure the pipeline starts there.
        Machine.Registers.PC = assembler.TextBase;
    }

    [Fact]
    public void Data_And_Text_Sections_Interaction()
    {
        // GOAL: Verify we can define data in .data, code in .text, and access the data.
        // The pipeline must fetch from 0x00400000 but load from 0x10010000.
        
        var source = @"
            .data
            var1: .word 0xDEADBEEF
            var2: .word 0xCAFEBABE
            
            .text
            .globl _start
            _start:
                la   x1, var1       # Load address of var1
                lw   x2, 0(x1)      # Load value (should be DEADBEEF)
                
                la   x3, var2       # Load address of var2
                lw   x4, 0(x3)      # Load value (should be CAFEBABE)
                
                add  x5, x2, x4     # Sum them up
                ebreak
        ";

        AssembleAndLoad(source);
        
        // Cycle until halt
        Cycle(50);
        
        Assert.True(Machine.Halted, "Machine did not halt.");
        AssertReg(2, 0xDEADBEEFul);
        AssertReg(4, 0xCAFEBABEul);
        
        // Sum check: DEADBEEF + CAFEBABE = 1A9AC79AD (truncated to 64-bit/32-bit depending on XLEN)
        // In 64-bit unsigned arithmetic:
        ulong sum = 0xDEADBEEFul + 0xCAFEBABEul;
        AssertReg(5, sum);
    }

    [Fact]
    public void Align_Directive_Correctness()
    {
        // GOAL: Verify .align creates the correct gaps in memory.
        // .align N aligns to 2^N.
        
        var source = @"
            .data
            b1: .byte 0x11
            
            .align 2        # Align to 4 bytes (2^2). Next address should be aligned.
            w1: .word 0x22222222
            
            .align 3        # Align to 8 bytes (2^3).
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

        // b1 is at base (e.g., 0x10010000)
        // w1 must be 4-byte aligned. b1 took 1 byte. Next slot is +1.
        // To align +1 to 4, we skip +1, +2, +3. w1 should be at base + 4.
        Assert.Equal(addrB1 + 4, addrW1);

        // d1 must be 8-byte aligned. 
        // w1 took 4 bytes (at base+4). Next slot is base+8.
        // base+8 IS 8-byte aligned. So d1 should be at base + 8.
        Assert.Equal(addrW1 + 4, addrD1);
        
        // Verify values in memory
        Assert.Equal(0x11u, Machine.Memory.ReadByte((uint)addrB1));
        Assert.Equal(0x22222222u, Machine.Memory.ReadWord((uint)addrW1));
    }

    [Fact]
    public void Space_Directive_Reservation()
    {
        // GOAL: Verify .space reserves bytes and label offsets are correct.
        
        var source = @"
            .data
            start:  .word 0xFFFFFFFF
            .space  12               # Reserve 12 bytes (3 words) of zero
            end:    .word 0xEEEEEEEE
            
            .text
            la x1, start
            la x2, end
            sub x3, x2, x1           # Calculate difference
            ebreak
        ";

        AssembleAndLoad(source);
        Cycle(20);

        // start takes 4 bytes.
        // space takes 12 bytes.
        // total gap = 4 + 12 = 16 bytes.
        AssertReg(3, 16ul);
        
        // Verify the space is zeroed (using raw memory check)
        ulong startAddr = Machine.Registers.Read(1);
        Assert.Equal(0u, Machine.Memory.ReadWord((uint)startAddr + 4));  // Space word 1
        Assert.Equal(0u, Machine.Memory.ReadWord((uint)startAddr + 8));  // Space word 2
        Assert.Equal(0u, Machine.Memory.ReadWord((uint)startAddr + 12)); // Space word 3
    }

    [Fact]
    public void BSS_Section_Zero_Initialization()
    {
        // GOAL: Verify .bss logic (mapped to data, usually aligned, zeroed).
        
        var source = @"
            .bss
            var_bss: .space 4
            
            .data
            var_data: .word 0xAAAA5555
            
            .text
            la x1, var_bss
            lw x2, 0(x1)     # Should be 0
            la x3, var_data
            lw x4, 0(x3)     # Should be 0xAAAA5555
            ebreak
        ";

        AssembleAndLoad(source);
        Cycle(20);

        AssertReg(2, 0ul);          // BSS is 0
        AssertReg(4, 0xAAAA5555ul); // Data is set
    }
}
