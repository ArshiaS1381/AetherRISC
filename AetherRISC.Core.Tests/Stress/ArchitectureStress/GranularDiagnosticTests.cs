using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class GranularDiagnosticTests
{
    [Fact]
    public void Assembler_Symbol_Table_Calculation()
    {
        string code = @"
            .data
            var1: .word 10
            var2: .word 20
            .text
            start: addi x1, x0, 1
        ";
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);

        // Check if labels are at expected RARS locations
        // .data starts at 0x10010000
        // .text starts at 0x00400000
        Assert.True(state.Memory.ReadWord(0x10010000) == 10, "var1 should be 10");
        Assert.True(state.Memory.ReadWord(0x10010004) == 20, "var2 should be 20");
    }

    [Fact]
    public void Linux_Handler_Write_Direct_Test()
    {
        // Tests the handler WITHOUT the pipeline/assembler
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var handler = new MultiOSHandler { Kind = OSKind.Linux };
        
        // Setup sys_write(stdout, buf, 4)
        state.Memory.WriteByte(100, (byte)'T');
        state.Memory.WriteByte(101, (byte)'E');
        state.Memory.WriteByte(102, (byte)'S');
        state.Memory.WriteByte(103, (byte)'T');
        
        state.Registers.Write(17, 64); // a7 = sys_write
        state.Registers.Write(10, 1);  // a0 = stdout
        state.Registers.Write(11, 100);// a1 = buf
        state.Registers.Write(12, 4);  // a2 = count

        handler.HandleEcall(state);

        Assert.Equal(4ul, state.Registers.Read(10)); // Should return count written
    }

    [Fact]
    public void Pseudo_LA_Expansion_Manual_Check()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        
        // Setup a label at 0x100
        string code = ".text \n la x1, 0x100";
        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);

        // LA should expand to LUI + ADDI
        // Check first instruction is LUI (opcode 0x37)
        uint inst1 = state.Memory.ReadWord(0);
        Assert.Equal(0x37u, inst1 & 0x7F);
    }
}
