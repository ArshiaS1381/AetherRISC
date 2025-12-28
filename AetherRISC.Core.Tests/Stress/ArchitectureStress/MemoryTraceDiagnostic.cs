using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class MemoryTraceDiagnostic
{
    private readonly ITestOutputHelper _output;
    public MemoryTraceDiagnostic(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Trace_Linux_Write_Binary_Layout()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        
        string code = @"
            .data
            msg: .asciz ""Linux!""
            .text
            li a0, 1
            la a1, msg
            li a2, 6
            li a7, 64
            ecall
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);

        _output.WriteLine("--- Instruction Trace ---");
        // Start loop from current PC
        for (ulong addr = state.ProgramCounter; addr < state.ProgramCounter + 40; addr += 4)
        {
            uint raw = state.Memory.ReadWord((uint)addr);
            _output.WriteLine($"Addr 0x{addr:X8}: 0x{raw:X8}");
        }
        
        _output.WriteLine("--- Data Trace ---");
        uint msgAddr = 0x10010000;
        _output.WriteLine($"Msg at 0x{msgAddr:X8}: '{(char)state.Memory.ReadByte(msgAddr)}{(char)state.Memory.ReadByte(msgAddr+1)}...'");
        
        Assert.NotEqual(0u, state.Memory.ReadWord((uint)state.ProgramCounter));
    }
}
