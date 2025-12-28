using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Tests.Unit.Pipeline; // <-- FIXED: Added missing namespace
using System.Collections.Generic;

namespace AetherRISC.Tests.Integration.Diagnostics;

public class DeepSystemProbe : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public DeepSystemProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Probe_Ble_Pseudo_Logic()
    {
        // REWRITE: Use text assembly to test the Pseudo Expansion logic properly.
        // Inst.Ble() does not exist because it is a pseudo-op.
        
        var source = @"
            .text
            li x1, 5
            li x2, 3
            
            # BLE 5, 3, target
            # 5 <= 3 is False. Should NOT take branch.
            ble x1, x2, target 
            
            # Success path
            li x3, 1
            j end
            
            target:
            li x3, 0   # Fail path
            
            end:
            ebreak
        ";

        InitPipeline();
        var asm = new AetherRISC.Core.Assembler.SourceAssembler(source);
        asm.Assemble(Machine);
        Machine.Registers.PC = asm.TextBase;

        Cycle(20);

        if (Machine.Registers.Read(3) == 0)
        {
             _output.WriteLine("CRITICAL FAILURE: BLE logic is inverted! (5 <= 3 evaluated as True)");
        }
        Assert.Equal(1ul, Machine.Registers.Read(3));
    }

    [Fact]
    public void Probe_Data_Alignment()
    {
        var source = @"
            .data
            .byte 0xFF
            .align 2        # Should align to next 4-byte boundary
            target: .word 0xDEADBEEF
            
            .text
            la x1, target
            lw x2, 0(x1)
            ebreak
        ";

        InitPipeline();
        var asm = new AetherRISC.Core.Assembler.SourceAssembler(source);
        asm.Assemble(Machine);
        Machine.Registers.PC = asm.TextBase;

        Cycle(10);

        ulong addr = Machine.Registers.Read(1);
        ulong val = Machine.Registers.Read(2);

        _output.WriteLine($"Target Address: {addr:X}");
        _output.WriteLine($"Loaded Value:   {val:X}");

        Assert.True(addr % 4 == 0, $"Address {addr:X} is not 4-byte aligned.");
        Assert.Equal(0xDEADBEEFul, val);
    }

    [Fact]
    public void Probe_Load_Load_Branch_CycleTrace()
    {
        InitPipeline();
        Machine.Memory.WriteWord(0x100, 10);
        Machine.Memory.WriteWord(0x104, 20);
        Machine.Registers.Write(1, 0x100);

        // 1. LW x2, 0(x1)
        Assembler.Add(pc => Inst.Lw(2, 1, 0));
        // 2. LW x3, 4(x1)
        Assembler.Add(pc => Inst.Lw(3, 1, 4));
        // 3. BEQ x2, x3, Target (Reads x2, x3). Needs Stall for x3!
        Assembler.Add(pc => Inst.Beq(2, 3, 8));
        
        Assembler.Add(pc => Inst.Addi(4, 0, 1)); // Fallthrough
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1));

        LoadProgram();

        Cycle(1); // Fetch LW1
        Cycle(1); // Dec LW1
        Cycle(1); // Ex LW1 (LW2 in Dec)
        Cycle(1); // Mem LW1, Ex LW2, Dec BEQ -> HAZARD POINT
        
        var decBuf = Pipeline.Buffers.DecodeExecute;
        var ifId = Pipeline.Buffers.FetchDecode;

        _output.WriteLine($"[Cycle 4] Dec->Ex IsEmpty? {decBuf.IsEmpty}");
        _output.WriteLine($"[Cycle 4] IF->Dec Stalled? {ifId.IsStalled}");

        Assert.True(decBuf.IsEmpty, "Pipeline failed to insert Bubble in Cycle 4 (Dec->Ex should be empty)");
        Assert.True(ifId.IsStalled, "Pipeline failed to Stall Fetch in Cycle 4");
    }

    [Fact]
    public void Probe_Store_Writeback()
    {
        InitPipeline();
        Machine.Registers.Write(1, 0x100);
        Machine.Registers.Write(2, 0xCAFE);

        Assembler.Add(pc => Inst.Sw(1, 2, 0)); 
        Assembler.Add(pc => Inst.Lw(3, 1, 0)); 
        
        LoadProgram();
        Cycle(10);

        Assert.Equal(0xCAFEu, Machine.Memory.ReadWord(0x100));
        Assert.Equal(0xCAFEul, Machine.Registers.Read(3));
    }
}
