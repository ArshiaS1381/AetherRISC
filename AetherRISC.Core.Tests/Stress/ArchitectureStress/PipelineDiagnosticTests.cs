using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class PipelineDiagnosticTests
{
    private readonly ITestOutputHelper _output;
    public PipelineDiagnosticTests(ITestOutputHelper output) => _output = output;

    // ========================================
    // LEVEL 0: Raw Encoding/Decoding Tests
    // ========================================

    [Fact]
    public void Encoding_Addi_Is_Correct()
    {
        var inst = new AddiInstruction(5, 0, 42); // ADDI t0, zero, 42
        uint encoded = InstructionEncoder.Encode(inst);
        
        _output.WriteLine($"Encoded ADDI: 0x{encoded:X8}");
        
        // Verify fields
        uint opcode = encoded & 0x7F;
        uint rd = (encoded >> 7) & 0x1F;
        uint funct3 = (encoded >> 12) & 0x7;
        uint rs1 = (encoded >> 15) & 0x1F;
        int imm = (int)encoded >> 20;
        
        Assert.Equal(0x13u, opcode);  // OP-IMM
        Assert.Equal(5u, rd);          // t0 = x5
        Assert.Equal(0u, funct3);      // ADDI
        Assert.Equal(0u, rs1);         // zero = x0
        Assert.Equal(42, imm);         // immediate
    }

    [Fact]
    public void Decoding_Addi_Is_Correct()
    {
        // Manually encode: ADDI x5, x0, 42
        // imm[11:0] = 42, rs1 = 0, funct3 = 0, rd = 5, opcode = 0x13
        uint encoded = (42u << 20) | (0u << 15) | (0u << 12) | (5u << 7) | 0x13u;
        
        var decoder = new InstructionDecoder();
        var inst = decoder.Decode(encoded);
        
        _output.WriteLine($"Decoded: {inst.Mnemonic} rd={inst.Rd} rs1={inst.Rs1} imm={inst.Imm}");
        
        Assert.Equal("ADDI", inst.Mnemonic);
        Assert.Equal(5, inst.Rd);
        Assert.Equal(0, inst.Rs1);
        Assert.Equal(42, inst.Imm);
    }

    [Fact]
    public void Encoding_Lui_Is_Correct()
    {
        var inst = new LuiInstruction(5, 0x12345000); // LUI t0, 0x12345
        uint encoded = InstructionEncoder.Encode(inst);
        
        _output.WriteLine($"Encoded LUI: 0x{encoded:X8}");
        
        uint opcode = encoded & 0x7F;
        uint rd = (encoded >> 7) & 0x1F;
        uint imm = encoded & 0xFFFFF000;
        
        Assert.Equal(0x37u, opcode);
        Assert.Equal(5u, rd);
        Assert.Equal(0x12345000u, imm);
    }

    [Fact]
    public void Encoding_Add_Is_Correct()
    {
        var inst = new AddInstruction(7, 5, 6); // ADD t2, t0, t1
        uint encoded = InstructionEncoder.Encode(inst);
        
        _output.WriteLine($"Encoded ADD: 0x{encoded:X8}");
        
        uint opcode = encoded & 0x7F;
        uint rd = (encoded >> 7) & 0x1F;
        uint funct3 = (encoded >> 12) & 0x7;
        uint rs1 = (encoded >> 15) & 0x1F;
        uint rs2 = (encoded >> 20) & 0x1F;
        uint funct7 = (encoded >> 25) & 0x7F;
        
        Assert.Equal(0x33u, opcode);
        Assert.Equal(7u, rd);
        Assert.Equal(0u, funct3);
        Assert.Equal(5u, rs1);
        Assert.Equal(6u, rs2);
        Assert.Equal(0u, funct7);
    }

    // ========================================
    // LEVEL 1: Direct Memory Write + Decode
    // ========================================

    [Fact]
    public void Assembler_Writes_Instructions_To_Memory()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            addi t0, zero, 42
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        uint inst0 = state.Memory.ReadWord(0);
        uint inst1 = state.Memory.ReadWord(4);

        _output.WriteLine($"Instruction at 0x0: 0x{inst0:X8}");
        _output.WriteLine($"Instruction at 0x4: 0x{inst1:X8}");

        // Verify first instruction is ADDI (opcode 0x13)
        Assert.Equal(0x13u, inst0 & 0x7F);
        // Verify second instruction is EBREAK (0x00100073)
        Assert.Equal(0x00100073u, inst1);
    }

    [Fact]
    public void Assembler_LI_Expands_Correctly()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            li t0, 10
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        uint inst0 = state.Memory.ReadWord(0);
        uint inst1 = state.Memory.ReadWord(4);
        uint inst2 = state.Memory.ReadWord(8);

        _output.WriteLine($"Instruction at 0x0: 0x{inst0:X8}");
        _output.WriteLine($"Instruction at 0x4: 0x{inst1:X8}");
        _output.WriteLine($"Instruction at 0x8: 0x{inst2:X8}");

        var decoder = new InstructionDecoder();
        var decoded0 = decoder.Decode(inst0);
        var decoded1 = decoder.Decode(inst1);

        _output.WriteLine($"Decoded[0]: {decoded0.Mnemonic} rd={decoded0.Rd} imm={decoded0.Imm}");
        _output.WriteLine($"Decoded[1]: {decoded1.Mnemonic} rd={decoded1.Rd} rs1={decoded1.Rs1} imm={decoded1.Imm}");

        // LI 10 should expand to LUI + ADDI
        Assert.Equal("LUI", decoded0.Mnemonic);
        Assert.Equal(5, decoded0.Rd); // t0 = x5
        Assert.Equal("ADDI", decoded1.Mnemonic);
        Assert.Equal(5, decoded1.Rd);
        Assert.Equal(5, decoded1.Rs1);
        Assert.Equal(10, decoded1.Imm);
    }

    // ========================================
    // LEVEL 2: Single Instruction Execution
    // ========================================

    [Fact]
    public void Single_Addi_Executes_Correctly()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            addi t0, zero, 42
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        _output.WriteLine($"PC after assemble: 0x{state.ProgramCounter:X}");

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(20);

        ulong result = state.Registers.Read(5); // t0 = x5
        _output.WriteLine($"t0 (x5) = {result}");

        Assert.Equal(42ul, result);
    }

    [Fact]
    public void Two_Independent_Addis_Execute_Correctly()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            addi t0, zero, 10
            addi t1, zero, 20
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(20);

        _output.WriteLine($"t0 = {state.Registers.Read(5)}");
        _output.WriteLine($"t1 = {state.Registers.Read(6)}");

        Assert.Equal(10ul, state.Registers.Read(5));
        Assert.Equal(20ul, state.Registers.Read(6));
    }

    // ========================================
    // LEVEL 3: RAW Hazard (Forwarding Required)
    // ========================================

    [Fact]
    public void RAW_Hazard_Addi_Chain_With_Forwarding()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        // t0 = 10, then t0 = t0 + 5 = 15 (RAW hazard on t0)
        string code = @"
            addi t0, zero, 10
            addi t0, t0, 5
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(20);

        _output.WriteLine($"t0 = {state.Registers.Read(5)}");

        Assert.Equal(15ul, state.Registers.Read(5));
    }

    [Fact]
    public void RAW_Hazard_Three_Instruction_Chain()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        // t0 = 10, t1 = t0 + 5 = 15, t2 = t1 + 3 = 18
        string code = @"
            addi t0, zero, 10
            addi t1, t0, 5
            addi t2, t1, 3
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(30);

        _output.WriteLine($"t0 = {state.Registers.Read(5)}");
        _output.WriteLine($"t1 = {state.Registers.Read(6)}");
        _output.WriteLine($"t2 = {state.Registers.Read(7)}");

        Assert.Equal(10ul, state.Registers.Read(5));
        Assert.Equal(15ul, state.Registers.Read(6));
        Assert.Equal(18ul, state.Registers.Read(7));
    }

    // ========================================
    // LEVEL 4: LUI + ADDI (LI Expansion)
    // ========================================

    [Fact]
    public void Lui_Alone_Works()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            lui t0, 0x12345
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(20);

        ulong result = state.Registers.Read(5);
        _output.WriteLine($"t0 = 0x{result:X}");

        // LUI loads upper 20 bits, so 0x12345 << 12 = 0x12345000
        Assert.Equal(0x12345000ul, result);
    }

    [Fact]
    public void Li_Small_Value_Works()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            li t0, 10
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(20);

        _output.WriteLine($"t0 = {state.Registers.Read(5)}");

        Assert.Equal(10ul, state.Registers.Read(5));
    }

    // ========================================
    // LEVEL 5: Register-Register Operations
    // ========================================

    [Fact]
    public void Add_Two_Registers()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            addi t0, zero, 10
            addi t1, zero, 20
            add t2, t0, t1
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(30);

        _output.WriteLine($"t0 = {state.Registers.Read(5)}");
        _output.WriteLine($"t1 = {state.Registers.Read(6)}");
        _output.WriteLine($"t2 = {state.Registers.Read(7)}");

        Assert.Equal(10ul, state.Registers.Read(5));
        Assert.Equal(20ul, state.Registers.Read(6));
        Assert.Equal(30ul, state.Registers.Read(7));
    }

    // ========================================
    // LEVEL 6: Memory Operations
    // ========================================

    [Fact]
    public void Store_Word_Works()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        string code = @"
            addi t0, zero, 123
            sw t0, 512(zero)
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(30);

        uint memValue = state.Memory.ReadWord(512);
        _output.WriteLine($"Memory[512] = {memValue}");

        Assert.Equal(123u, memValue);
    }

    [Fact]
    public void Load_Word_Works()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);

        // Pre-write a value to memory
        state.Memory.WriteWord(512, 999);

        string code = @"
            lw t0, 512(zero)
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(20);

        _output.WriteLine($"t0 = {state.Registers.Read(5)}");

        // Note: LW sign-extends, so if value fits in 32 bits positive, should be same
        Assert.Equal(999ul, state.Registers.Read(5));
    }
}
