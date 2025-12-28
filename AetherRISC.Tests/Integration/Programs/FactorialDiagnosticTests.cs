using Xunit;
using Xunit.Abstractions;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Integration.Programs;

public class FactorialDiagnosticTests : CpuTestFixture
{
    private readonly ITestOutputHelper _output;
    
    public FactorialDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Diag_Mul_Basic()
    {
        // Test that MUL works at all
        Init64();
        Assembler.Add(_ => Inst.Mul(12, 10, 11));  // mul a2, a0, a1
        
        Machine.Registers.Write(10, 5);  // a0 = 5
        Machine.Registers.Write(11, 4);  // a1 = 4
        
        Run(1);
        
        ulong result = Machine.Registers.Read(12);
        _output.WriteLine($"MUL result: {result} (expected 20)");
        AssertReg(12, 20ul);
    }

    [Fact]
    public void Diag_Mul_Encoding()
    {
        // Verify MUL encodes with correct funct7
        var mul = Inst.Mul(12, 10, 11);
        uint encoded = InstructionEncoder.Encode(mul);
        
        uint opcode = encoded & 0x7F;
        uint funct3 = (encoded >> 12) & 0x7;
        uint funct7 = (encoded >> 25) & 0x7F;
        
        _output.WriteLine($"MUL encoded: 0x{encoded:X8}");
        _output.WriteLine($"  opcode: 0x{opcode:X2} (expected 0x33)");
        _output.WriteLine($"  funct3: {funct3} (expected 0)");
        _output.WriteLine($"  funct7: {funct7} (expected 1)");
        
        Assert.Equal(0x33u, opcode);
        Assert.Equal(0u, funct3);
        Assert.Equal(1u, funct7);  // MUL has funct7 = 1
    }

    [Fact]
    public void Diag_SD_LD_Roundtrip()
    {
        // Test that SD/LD preserves values correctly
        Init64();
        Machine.Registers.Write(2, 0x1000);  // sp = 0x1000
        Machine.Registers.Write(10, 0xDEADBEEF12345678);  // a0 = test value
        
        string code = @"
            .text
            sd a0, 0(sp)
            ld a1, 0(sp)
        ";
        
        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;
        Runner.Run(2);
        
        ulong stored = Machine.Registers.Read(10);
        ulong loaded = Machine.Registers.Read(11);
        _output.WriteLine($"Original a0: 0x{stored:X16}");
        _output.WriteLine($"Loaded a1:   0x{loaded:X16}");
        
        AssertReg(11, 0xDEADBEEF12345678ul);
    }

    [Fact]
    public void Diag_BGT_Branch_Taken()
    {
        // Test BGT pseudo-instruction
        Init64();
        
        string code = @"
            .text
            li a0, 5
            li t1, 1
            bgt a0, t1, target
            li a2, 999
            j end
        target:
            li a2, 1
        end:
            nop
        ";
        
        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;
        Runner.Run(10);
        
        ulong result = Machine.Registers.Read(12);
        _output.WriteLine($"a2 = {result} (expected 1 if branch taken)");
        AssertReg(12, 1ul);  // Branch should be taken
    }

    [Fact]
    public void Diag_BGT_Branch_Not_Taken()
    {
        // Test BGT when condition is false
        Init64();
        
        string code = @"
            .text
            li a0, 1
            li t1, 1
            bgt a0, t1, target
            li a2, 1
            j end
        target:
            li a2, 999
        end:
            nop
        ";
        
        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;
        Runner.Run(10);
        
        ulong result = Machine.Registers.Read(12);
        _output.WriteLine($"a2 = {result} (expected 1 if branch NOT taken)");
        AssertReg(12, 1ul);  // Branch should NOT be taken (1 > 1 is false)
    }

    [Fact]
    public void Diag_Simple_Factorial_No_Recursion()
    {
        // Compute 5*4*3*2*1 without recursion to verify MUL works
        Init64();
        
        string code = @"
            .text
            li a0, 1
            li t0, 2
            mul a0, a0, t0
            li t0, 3
            mul a0, a0, t0
            li t0, 4
            mul a0, a0, t0
            li t0, 5
            mul a0, a0, t0
        ";
        
        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;
        Runner.Run(20);
        
        ulong result = Machine.Registers.Read(10);
        _output.WriteLine($"5! = {result} (expected 120)");
        AssertReg(10, 120ul);
    }

    [Fact]
    public void Diag_JAL_JALR_Return()
    {
        // Test JAL/RET work correctly
        Init64();
        Machine.Registers.Write(2, 0x100000);  // SP
        
        string code = @"
            .text
            li a0, 0
            jal ra, func
            li a0, 42
            j end
        func:
            li t0, 1
            ret
        end:
            nop
        ";
        
        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;
        Runner.Run(10);
        
        ulong result = Machine.Registers.Read(10);
        _output.WriteLine($"a0 = {result} (expected 42 if returned correctly)");
        AssertReg(10, 42ul);
    }

    [Fact]
    public void Diag_Stack_Push_Pop()
    {
        // Test stack operations
        Init64();
        Machine.Registers.Write(2, 0x100000);  // SP = 1MB
        
        string code = @"
            .text
            li a0, 123
            li a1, 456
            addi sp, sp, -16
            sd a0, 0(sp)
            sd a1, 8(sp)
            li a0, 0
            li a1, 0
            ld a2, 0(sp)
            ld a3, 8(sp)
            addi sp, sp, 16
        ";
        
        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;
        Runner.Run(20);
        
        _output.WriteLine($"a2 = {Machine.Registers.Read(12)} (expected 123)");
        _output.WriteLine($"a3 = {Machine.Registers.Read(13)} (expected 456)");
        _output.WriteLine($"SP = 0x{Machine.Registers.Read(2):X} (expected 0x100000)");
        
        AssertReg(12, 123ul);
        AssertReg(13, 456ul);
        AssertReg(2, 0x100000ul);
    }
}
