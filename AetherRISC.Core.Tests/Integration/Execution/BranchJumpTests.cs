using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

namespace AetherRISC.Core.Tests.Integration;

public class BranchJumpTests
{
    private (MachineState state, PipelineController pipeline, TestAssembler asm) Setup()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        state.ProgramCounter = 0;
        return (state, new PipelineController(state), new TestAssembler());
    }

    private void LoadAndRun(MachineState state, PipelineController pipeline, TestAssembler asm, int cycles)
    {
        var insts = asm.Assemble();
        for (int i = 0; i < insts.Count; i++)
            state.Memory!.WriteWord((uint)(i * 4), InstructionEncoder.Encode(insts[i]));
        for (int i = 0; i < cycles; i++) pipeline.Cycle();
    }

    // ==================== JAL TESTS ====================

    [Fact]
    public void Jal_ForwardJump_SkipsInstructions()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 1));
        asm.Add(pc => Inst.Jal(0, asm.To("target", pc)));
        asm.Add(pc => Inst.Addi(10, 0, 99));                      // SKIP
        asm.Add(pc => Inst.Addi(11, 0, 2), "target");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 25);

        Assert.Equal((ulong)1, state.Registers.Read(10));
        Assert.Equal((ulong)2, state.Registers.Read(11));
    }

    [Fact]
    public void Jal_SavesReturnAddress()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Jal(1, asm.To("target", pc)));         // 0x00: x1 = 0x04
        asm.Add(pc => Inst.Nop());                                // 0x04
        asm.Add(pc => Inst.Nop(), "target");                      // 0x08
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 20);

        Assert.Equal((ulong)0x04, state.Registers.Read(1));
    }

    [Fact]
    public void Jal_BackwardJump()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 1));
        asm.Add(pc => Inst.Jal(0, asm.To("skip", pc)));
        asm.Add(pc => Inst.Addi(10, 10, 10), "back");             // 0x08: add 10
        asm.Add(pc => Inst.Jal(0, asm.To("done", pc)));
        asm.Add(pc => Inst.Jal(0, asm.To("back", pc)), "skip");   // jump back
        asm.Add(pc => Inst.Nop(), "done");

        LoadAndRun(state, pipeline, asm, 40);

        Assert.Equal((ulong)11, state.Registers.Read(10));
    }

    // ==================== JALR TESTS ====================

    [Fact]
    public void Jalr_IndirectJump()
    {
        // FIX: x10 = 20 (0x14) to jump PAST instruction at 0x10
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 20));                      // 0x00: x10 = 0x14 (target)
        asm.Add(pc => Inst.Nop());                                // 0x04
        asm.Add(pc => Inst.Nop());                                // 0x08
        asm.Add(pc => Inst.Jalr(0, 10, 0));                       // 0x0C: jump to x10 = 0x14
        asm.Add(pc => Inst.Addi(11, 0, 99));                      // 0x10: SKIP
        asm.Add(pc => Inst.Addi(12, 0, 42));                      // 0x14: TARGET
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(11));         // Skipped
        Assert.Equal((ulong)42, state.Registers.Read(12));        // Executed
    }

    [Fact]
    public void Jalr_WithOffset()
    {
        // x10 = 12, offset = 8, target = 12 + 8 = 20 = 0x14
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 12));                      // 0x00: x10 = 12
        asm.Add(pc => Inst.Nop());                                // 0x04
        asm.Add(pc => Inst.Nop());                                // 0x08
        asm.Add(pc => Inst.Jalr(0, 10, 8));                       // 0x0C: jump to 12+8=20=0x14
        asm.Add(pc => Inst.Addi(11, 0, 99));                      // 0x10: SKIP
        asm.Add(pc => Inst.Addi(12, 0, 42));                      // 0x14: TARGET
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(11));         // Skipped
        Assert.Equal((ulong)42, state.Registers.Read(12));        // Executed
    }

    [Fact]
    public void Jalr_FunctionCallAndReturn()
    {
        // Simulated function call: JAL to function, JALR to return
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));                       // 0x00: arg
        asm.Add(pc => Inst.Jal(1, asm.To("func", pc)));           // 0x04: call, ra=0x08
        asm.Add(pc => Inst.Addi(12, 0, 99));                      // 0x08: return point (after call)
        asm.Add(pc => Inst.Jal(0, asm.To("end", pc)));            // 0x0C: skip function
        asm.Add(pc => Inst.Add(11, 10, 10), "func");              // 0x10: x11 = x10 * 2
        asm.Add(pc => Inst.Jalr(0, 1, 0));                        // 0x14: return
        asm.Add(pc => Inst.Nop(), "end");

        LoadAndRun(state, pipeline, asm, 50);

        Assert.Equal((ulong)10, state.Registers.Read(11));        // 5 * 2
        Assert.Equal((ulong)99, state.Registers.Read(12));        // returned here
    }

    // ==================== BEQ TESTS ====================

    [Fact]
    public void Beq_Taken_WhenEqual()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));
        asm.Add(pc => Inst.Addi(11, 0, 5));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Beq(10, 11, asm.To("taken", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 99));                      // SKIP
        asm.Add(pc => Inst.Addi(13, 0, 42), "taken");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    [Fact]
    public void Beq_NotTaken_WhenNotEqual()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));
        asm.Add(pc => Inst.Addi(11, 0, 7));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Beq(10, 11, asm.To("skip", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 77));                      // EXECUTE
        asm.Add(pc => Inst.Addi(13, 0, 42), "skip");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)77, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    // ==================== BNE TESTS ====================

    [Fact]
    public void Bne_Taken_WhenNotEqual()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));
        asm.Add(pc => Inst.Addi(11, 0, 7));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bne(10, 11, asm.To("taken", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 99));
        asm.Add(pc => Inst.Addi(13, 0, 42), "taken");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    [Fact]
    public void Bne_NotTaken_WhenEqual()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));
        asm.Add(pc => Inst.Addi(11, 0, 5));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bne(10, 11, asm.To("skip", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 77));
        asm.Add(pc => Inst.Addi(13, 0, 42), "skip");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)77, state.Registers.Read(12));
    }

    // ==================== BLT/BGE TESTS ====================

    [Fact]
    public void Blt_Taken_WhenLessThan()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 3));
        asm.Add(pc => Inst.Addi(11, 0, 7));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Blt(10, 11, asm.To("taken", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 99));
        asm.Add(pc => Inst.Addi(13, 0, 42), "taken");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    [Fact]
    public void Blt_NotTaken_WhenGreaterOrEqual()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 7));
        asm.Add(pc => Inst.Addi(11, 0, 3));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Blt(10, 11, asm.To("skip", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 77));
        asm.Add(pc => Inst.Addi(13, 0, 42), "skip");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)77, state.Registers.Read(12));
    }

    [Fact]
    public void Blt_Signed_NegativeLessThanPositive()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, -5));                      // -5 (signed)
        asm.Add(pc => Inst.Addi(11, 0, 3));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Blt(10, 11, asm.To("taken", pc)));     // -5 < 3 ? YES
        asm.Add(pc => Inst.Addi(12, 0, 99));
        asm.Add(pc => Inst.Addi(13, 0, 42), "taken");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    [Fact]
    public void Bge_Taken_WhenGreater()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 10));
        asm.Add(pc => Inst.Addi(11, 0, 5));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bge(10, 11, asm.To("taken", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 99));
        asm.Add(pc => Inst.Addi(13, 0, 42), "taken");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    [Fact]
    public void Bge_Taken_WhenEqual()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));
        asm.Add(pc => Inst.Addi(11, 0, 5));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bge(10, 11, asm.To("taken", pc)));
        asm.Add(pc => Inst.Addi(12, 0, 99));
        asm.Add(pc => Inst.Addi(13, 0, 42), "taken");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    // ==================== BLTU/BGEU (UNSIGNED) ====================

    [Fact]
    public void Bltu_UnsignedComparison()
    {
        // -1 as unsigned is MAX, so -1 >= 5 in unsigned
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, -1));                      // MAX unsigned
        asm.Add(pc => Inst.Addi(11, 0, 5));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bltu(10, 11, asm.To("skip", pc)));     // MAX < 5 ? NO
        asm.Add(pc => Inst.Addi(12, 0, 77));                      // EXECUTE
        asm.Add(pc => Inst.Addi(13, 0, 42), "skip");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)77, state.Registers.Read(12));
    }

    [Fact]
    public void Bgeu_UnsignedComparison()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, -1));                      // MAX unsigned
        asm.Add(pc => Inst.Addi(11, 0, 5));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bgeu(10, 11, asm.To("taken", pc)));    // MAX >= 5 ? YES
        asm.Add(pc => Inst.Addi(12, 0, 99));
        asm.Add(pc => Inst.Addi(13, 0, 42), "taken");

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)0, state.Registers.Read(12));
        Assert.Equal((ulong)42, state.Registers.Read(13));
    }

    // ==================== NESTED LOOPS ====================

    [Fact]
    public void NestedLoop_3x3()
    {
        // Outer loop 3 times, inner loop 3 times each = 9 increments
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 0));                       // x10 = counter
        asm.Add(pc => Inst.Addi(11, 0, 3));                       // x11 = outer = 3
        asm.Add(pc => Inst.Beq(11, 0, asm.To("done", pc)), "outer");
        asm.Add(pc => Inst.Addi(12, 0, 3));                       // x12 = inner = 3
        asm.Add(pc => Inst.Beq(12, 0, asm.To("outer_dec", pc)), "inner");
        asm.Add(pc => Inst.Addi(10, 10, 1));                      // counter++
        asm.Add(pc => Inst.Addi(12, 12, -1));                     // inner--
        asm.Add(pc => Inst.Jal(0, asm.To("inner", pc)));
        asm.Add(pc => Inst.Addi(11, 11, -1), "outer_dec");        // outer--
        asm.Add(pc => Inst.Jal(0, asm.To("outer", pc)));
        asm.Add(pc => Inst.Nop(), "done");

        LoadAndRun(state, pipeline, asm, 300);

        Assert.Equal((ulong)9, state.Registers.Read(10));
    }
}
