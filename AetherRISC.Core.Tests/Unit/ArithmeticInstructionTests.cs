using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

namespace AetherRISC.Core.Tests.Unit;

public class ArithmeticInstructionTests
{
    private (MachineState state, PipelineController pipeline) Setup()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        state.ProgramCounter = 0;
        return (state, new PipelineController(state));
    }

    private void RunPipeline(PipelineController pipeline, int cycles = 20)
    {
        for (int i = 0; i < cycles; i++) pipeline.Cycle();
    }

    private void LoadAndRun(MachineState state, PipelineController pipeline, TestAssembler asm, int cycles = 20)
    {
        var insts = asm.Assemble();
        for (int i = 0; i < insts.Count; i++)
            state.Memory!.WriteWord((uint)(i * 4), InstructionEncoder.Encode(insts[i]));
        RunPipeline(pipeline, cycles);
    }

    [Fact]
    public void Add_TwoPositives()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 10));
        asm.Add(pc => Inst.Addi(11, 0, 20));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Add(12, 10, 11));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)30, state.Registers.Read(12));
    }

    [Fact]
    public void Sub_PositiveResult()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 50));
        asm.Add(pc => Inst.Addi(11, 0, 20));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Sub(12, 10, 11));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)30, state.Registers.Read(12));
    }

    [Fact]
    public void And_BitwiseOperation()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 0b1100));
        asm.Add(pc => Inst.Addi(11, 0, 0b1010));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.And(12, 10, 11));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)0b1000, state.Registers.Read(12));
    }

    [Fact]
    public void Or_BitwiseOperation()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 0b1100));
        asm.Add(pc => Inst.Addi(11, 0, 0b1010));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Or(12, 10, 11));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)0b1110, state.Registers.Read(12));
    }

    [Fact]
    public void Xor_BitwiseOperation()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 0b1100));
        asm.Add(pc => Inst.Addi(11, 0, 0b1010));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Xor(12, 10, 11));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)0b0110, state.Registers.Read(12));
    }

    [Fact]
    public void Sll_ShiftLeft()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 1));
        asm.Add(pc => Inst.Addi(11, 0, 4));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Sll(12, 10, 11));  // 1 << 4 = 16
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)16, state.Registers.Read(12));
    }

    [Fact]
    public void Srl_ShiftRightLogical()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 64));
        asm.Add(pc => Inst.Addi(11, 0, 2));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Srl(12, 10, 11));  // 64 >> 2 = 16
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)16, state.Registers.Read(12));
    }

    [Fact]
    public void Slt_SetLessThan_True()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 5));
        asm.Add(pc => Inst.Addi(11, 0, 10));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Slt(12, 10, 11));  // 5 < 10 -> 1
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)1, state.Registers.Read(12));
    }

    [Fact]
    public void Slt_SetLessThan_False()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Addi(10, 0, 15));
        asm.Add(pc => Inst.Addi(11, 0, 10));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Slt(12, 10, 11));  // 15 < 10 -> 0
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 25);
        Assert.Equal((ulong)0, state.Registers.Read(12));
    }

    [Fact]
    public void Lui_LoadUpperImmediate()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Lui(10, 0x12345000));  // Upper 20 bits
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 20);
        Assert.Equal((ulong)0x12345000, state.Registers.Read(10));
    }

    [Fact]
    public void Auipc_AddUpperImmediateToPC()
    {
        var (state, pipeline) = Setup();
        var asm = new TestAssembler();
        asm.Add(pc => Inst.Auipc(10, 0x1000));  // PC=0 + 0x1000
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        LoadAndRun(state, pipeline, asm, 20);
        Assert.Equal((ulong)0x1000, state.Registers.Read(10));
    }
}
