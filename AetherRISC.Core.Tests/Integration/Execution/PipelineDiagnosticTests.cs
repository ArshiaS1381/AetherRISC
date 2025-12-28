using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

namespace AetherRISC.Core.Tests.Integration;

public class PipelineDiagnosticTests
{
    [Fact]
    public void SimpleAddi_WritesCorrectValue()
    {
        // Just ADDI x10, x0, 42 followed by NOP padding
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();
        state.ProgramCounter = 0;

        asm.Add(pc => Inst.Addi(10, 0, 42));  // x10 = 42
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        var insts = asm.Assemble();
        for (int i = 0; i < insts.Count; i++)
            state.Memory.WriteWord((uint)(i * 4), InstructionEncoder.Encode(insts[i]));

        // Run enough cycles for ADDI to reach writeback (5 cycles for 5-stage pipeline)
        for (int i = 0; i < 10; i++) pipeline.Cycle();

        Assert.Equal((ulong)42, state.Registers.Read(10));
    }

    [Fact]
    public void TwoAddis_WithDependency_ForwardsCorrectly()
    {
        // ADDI x10, x0, 10 -> ADDI x11, x10, 5 (x11 should be 15)
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();
        state.ProgramCounter = 0;

        asm.Add(pc => Inst.Addi(10, 0, 10)); // x10 = 10
        asm.Add(pc => Inst.Addi(11, 10, 5)); // x11 = x10 + 5 = 15 (needs forwarding)
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        var insts = asm.Assemble();
        for (int i = 0; i < insts.Count; i++)
            state.Memory.WriteWord((uint)(i * 4), InstructionEncoder.Encode(insts[i]));

        for (int i = 0; i < 15; i++) pipeline.Cycle();

        Assert.Equal((ulong)10, state.Registers.Read(10));
        Assert.Equal((ulong)15, state.Registers.Read(11));
    }

    [Fact]
    public void SimpleMul_WritesCorrectValue()
    {
        // x10 = 3, x11 = 4, MUL x12, x10, x11 -> x12 = 12
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();
        state.ProgramCounter = 0;

        asm.Add(pc => Inst.Addi(10, 0, 3));   // x10 = 3
        asm.Add(pc => Inst.Addi(11, 0, 4));   // x11 = 4
        asm.Add(pc => Inst.Nop());            // Let values commit
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Mul(12, 10, 11));  // x12 = 3 * 4 = 12
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        var insts = asm.Assemble();
        for (int i = 0; i < insts.Count; i++)
            state.Memory.WriteWord((uint)(i * 4), InstructionEncoder.Encode(insts[i]));

        for (int i = 0; i < 20; i++) pipeline.Cycle();

        Assert.Equal((ulong)3, state.Registers.Read(10));
        Assert.Equal((ulong)4, state.Registers.Read(11));
        Assert.Equal((ulong)12, state.Registers.Read(12));
    }

    [Fact]
    public void MulWithSameRdAndRs1_AccumulatesCorrectly()
    {
        // This tests the specific pattern used in factorial: MUL x10, x10, x11
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();
        state.ProgramCounter = 0;

        asm.Add(pc => Inst.Addi(10, 0, 5));   // x10 = 5
        asm.Add(pc => Inst.Addi(11, 0, 3));   // x11 = 3
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Mul(10, 10, 11));  // x10 = 5 * 3 = 15
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        var insts = asm.Assemble();
        for (int i = 0; i < insts.Count; i++)
            state.Memory.WriteWord((uint)(i * 4), InstructionEncoder.Encode(insts[i]));

        for (int i = 0; i < 20; i++) pipeline.Cycle();

        Assert.Equal((ulong)15, state.Registers.Read(10));
    }
}
