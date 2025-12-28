using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class DeepPipelineDiagnostics
{
    private readonly ITestOutputHelper _output;
    public DeepPipelineDiagnostics(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Trace_LUI_Bit_Integrity()
    {
        // 1. Check Construction
        var lui = new LuiInstruction(5, 0x12345000);
        _output.WriteLine($"1. Instruction Object: Imm=0x{lui.Imm:X}");
        Assert.Equal(0x12345000, lui.Imm);

        // 2. Check Encoding
        uint encoded = InstructionEncoder.Encode(lui);
        _output.WriteLine($"2. Encoded Hex: 0x{encoded:X8}");
        Assert.Equal(0x123452B7u, encoded);

        // 3. Check Decoding
        var decoder = new InstructionDecoder();
        var decoded = decoder.Decode(encoded);
        _output.WriteLine($"3. Decoded Object: Mnemonic={decoded.Mnemonic}, Rd={decoded.Rd}, Imm=0x{decoded.Imm:X}");
        Assert.Equal("LUI", decoded.Mnemonic);
        Assert.Equal(5, decoded.Rd);
        Assert.Equal(0x12345000, decoded.Imm);

        // 4. Check Pipeline Execution (Bypass Assembler)
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        state.ProgramCounter = 0;
        state.Memory.WriteWord(0, encoded);
        state.Memory.WriteWord(4, 0x00100073); // EBREAK
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(15);
        
        ulong result = state.Registers.Read(5);
        _output.WriteLine($"4. Pipeline Result in x5: 0x{result:X}");
        Assert.Equal(0x12345000ul, result);
    }
}
