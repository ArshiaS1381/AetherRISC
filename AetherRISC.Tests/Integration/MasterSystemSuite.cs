using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Tests.Unit.Pipeline; 

namespace AetherRISC.Tests.Integration;

public class MasterSystemSuite : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public MasterSystemSuite(ITestOutputHelper output) => _output = output;

    private void AssembleAndLoad(string source)
    {
        InitPipeline();
        var assembler = new SourceAssembler(source);
        assembler.Assemble(Machine);
        Machine.Registers.PC = assembler.TextBase;
    }

    [Fact]
    public void The_Kitchen_Sink_Algorithm()
    {
        var source = @"
            .data
            .align 4
            array_base: 
                .word 0x00000005
                .word 0x00000003
                .word 0x00000001
                .word 0x00000004
                .word 0x00000002
            array_size: 
                .word 5
            hash_result:
                .space 8
            
            .text
            .globl _start
            _start:
                la x1, array_base
                la x10, array_size
                lw x2, 0(x10)
                
            sort_loop_outer:
                addi x3, x0, 0
                addi x4, x0, 0
                addi x5, x2, -1
                beq x2, x0, sort_done
                
            sort_loop_inner:
                bge x4, x5, check_swap
                slli x6, x4, 2
                add  x7, x1, x6
                lw   x8, 0(x7)
                lw   x9, 4(x7)
                ble x8, x9, no_swap
                sw x9, 0(x7)
                sw x8, 4(x7)
                addi x3, x0, 1
                
            no_swap:
                addi x4, x4, 1
                jal x0, sort_loop_inner
                
            check_swap:
                bne x3, x0, sort_loop_outer
                
            sort_done:
                addi x11, x0, 0x123
                addi x4, x0, 0
                
            hash_loop:
                bge x4, x2, finish
                slli x6, x4, 2
                add  x7, x1, x6
                lw   x8, 0(x7)
                add x11, x11, x8
                addi x12, x0, 0x1F
                clmul x11, x11, x12 
                addi x12, x0, 63
                bset x11, x11, x12
                mul x11, x11, x8
                addi x4, x4, 1
                jal x0, hash_loop
                
            finish:
                la x20, hash_result
                sd x11, 0(x20)
                ebreak
        ";

        AssembleAndLoad(source);
        
        int cycles = 0;
        while (!Machine.Halted && cycles < 5000)
        {
            Cycle();
            cycles++;
        }
        
        uint baseAddr = (uint)Machine.Registers.Read(1);
        Assert.Equal(1u, Machine.Memory.ReadWord(baseAddr + 0));
        Assert.Equal(2u, Machine.Memory.ReadWord(baseAddr + 4));
        Assert.Equal(3u, Machine.Memory.ReadWord(baseAddr + 8));
        Assert.Equal(4u, Machine.Memory.ReadWord(baseAddr + 12));
        Assert.Equal(5u, Machine.Memory.ReadWord(baseAddr + 16));
        
        ulong hash = Machine.Registers.Read(11);
        _output.WriteLine($"Final Hash: {hash:X16}");
        Assert.True(hash != 0, "Hash should not be zero");
    }

    [Fact]
    public void The_Hazard_Marathon()
    {
        var source = @"
            .text
            .globl _start
            _start:
                addi x1, x0, 10
                addi x2, x0, 20
                la   x10, var_mem
                sw   x1, 0(x10)
                lw   x3, 0(x10)
                add  x4, x3, x1
                add  x5, x4, x1
                nop
                nop 
                add  x6, x5, x1
                beq  x0, x0, skip
                addi x7, x0, 666
                addi x7, x0, 666
            skip:
                addi x7, x0, 1
                jal  x0, finish
                addi x8, x0, 999
            var_mem:
                .word 0
            finish:
                addi x8, x0, 100
                ebreak
        ";

        AssembleAndLoad(source);
        Cycle(50);
        
        AssertReg(3, 10ul);
        AssertReg(4, 20ul);
        AssertReg(5, 30ul);
        AssertReg(6, 40ul);
        AssertReg(7, 1ul);
        AssertReg(8, 100ul);
    }
}
