using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using Xunit;
using Xunit.Abstractions;
using System.Linq;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class DeepSystemProbeTests
{
    private readonly ITestOutputHelper _output;
    public DeepSystemProbeTests(ITestOutputHelper output) => _output = output;

    // --- PROBE 1: THE ASSEMBLER & TOKENIZER ---
    [Theory]
    [InlineData("lui t0, 0x12345", "LUI", 5, 0, 0x12345000)]
    // FIX: Changed value to 0x12345000 so it generates a single LUI 0x12345
    [InlineData("li t0, 0x12345000", "LUI", 5, 0, 0x12345000)] 
    [InlineData("sw t0, 512(sp)", "SW", 2, 5, 512)] // rs1=sp(2), rs2=t0(5)
    public void Probe_Assembler_Logic(string line, string expectedMnemonic, int r1, int r2, int expectedImm = -1)
    {
        _output.WriteLine($"Testing: {line}");
        _output.WriteLine($"Expect Params: r1={r1}, r2={r2}, imm={expectedImm}");

        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        state.Registers.Write(2, 0x1000); // Init sp

        var asm = new SourceAssembler(line) { TextBase = 0 };
        asm.Assemble(state); 

        // Read the 32-bit instruction we just assembled at address 0
        uint encoded = state.Memory.ReadWord(0);
        
        var decoded = new InstructionDecoder().Decode(encoded);
        
        _output.WriteLine($"Decoded: {decoded.Mnemonic} Rd:{decoded.Rd} Rs1:{decoded.Rs1} Rs2:{decoded.Rs2} Imm:0x{decoded.Imm:X}");

        Assert.Equal(expectedMnemonic, decoded.Mnemonic);
        
        if (expectedImm != -1) 
        {
            Assert.Equal(expectedImm, decoded.Imm);
        }

        if (decoded.Mnemonic == "SW")
        {
            Assert.Equal(r1, decoded.Rs1); 
            Assert.Equal(r2, decoded.Rs2); 
        }
        else if (decoded.Mnemonic == "LUI")
        {
            Assert.Equal(r1, decoded.Rd); 
        }
    }

    // --- PROBE 2: THE ENCODER BIT PATTERNS ---
    [Fact]
    public void Probe_Encoder_U_Type_Masking()
    {
        var lui = new LuiInstruction(5, 0x12345000);
        uint encoded = InstructionEncoder.Encode(lui);
        _output.WriteLine($"LUI Encoding: Expected 0x123452B7, Got 0x{encoded:X8}");
        Assert.Equal(0x123452B7u, encoded);
    }

    // --- PROBE 3: THE DECODER TYPE CONVERSION ---
    [Fact]
    public void Probe_Decoder_Sign_Extension()
    {
        uint raw = 0xFFFFF2B7; // LUI x5, 0xFFFFF000 (Negative Immediate)
        var decoder = new InstructionDecoder();
        var inst = decoder.Decode(raw);

        _output.WriteLine($"Decoded Imm: 0x{inst.Imm:X}");
        Assert.Equal(unchecked((int)0xFFFFF000), inst.Imm);
    }

    // --- PROBE 4: PIPELINE HAZARD FORWARDING ---
    [Fact]
    public void Probe_Pipeline_Forwarding_Timing()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        
        string code = @"
            addi x5, x0, 10
            addi x6, x5, 1
            addi x7, x5, 2
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(state);
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(20);

        _output.WriteLine($"x5: {state.Registers.Read(5)}");
        _output.WriteLine($"x6: {state.Registers.Read(6)}");
        _output.WriteLine($"x7: {state.Registers.Read(7)}");

        Assert.Equal((ulong)10, state.Registers.Read(5));
        Assert.Equal((ulong)11, state.Registers.Read(6));
        Assert.Equal((ulong)12, state.Registers.Read(7));
    }
}