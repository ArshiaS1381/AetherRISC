using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Tests.Unit.Pipeline; // <-- Added this missing namespace

namespace AetherRISC.Tests.Integration;

public class MasterSystemSuite : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public MasterSystemSuite(ITestOutputHelper output) => _output = output;

    private void AssembleAndLoad(string source)
    {
        InitPipeline();
        var assembler = new SourceAssembler(source);
        assembler.Assemble(Machine);
        Machine.Registers.PC = assembler.TextBase;
    }

    [Fact]
    public void The_Kitchen_Sink_Algorithm()
    {
        // -----------------------------------------------------------
        // SCENARIO: Sort an array, then calculate a hash of the result.
        // COVERS: Directives, Base I, RV64M, Zbs (Bit manip), Zbc (Clmul)
        // -----------------------------------------------------------
        
        var source = @"
            .data
            .align 4
            array_base: 
                .word 0x00000005    # Element 0
                .word 0x00000003    # Element 1
                .word 0x00000001    # Element 2
                .word 0x00000004    # Element 3
                .word 0x00000002    # Element 4
            array_size: 
                .word 5
            hash_result:
                .space 8            # Reserve space for final hash
            
            .text
            .globl _start
            _start:
                # --- PHASE 1: BUBBLE SORT ---
                # x1 = array base address
                # x2 = array size
                # x3 = swap flag
                
                la x1, array_base
                la x10, array_size
                lw x2, 0(x10)       # x2 = 5
                
            sort_loop_outer:
                addi x3, x0, 0      # swap flag = 0
                addi x4, x0, 0      # index = 0
                addi x5, x2, -1     # limit = size - 1
                
                beq x2, x0, sort_done   # Safety check
                
            sort_loop_inner:
                bge x4, x5, check_swap
                
                # Load array[i] and array[i+1]
                slli x6, x4, 2      # offset = i * 4
                add  x7, x1, x6     # ptr = base + offset
                lw   x8, 0(x7)      # val1
                lw   x9, 4(x7)      # val2
                
                # Compare
                ble x8, x9, no_swap
                
                # Swap
                sw x9, 0(x7)
                sw x8, 4(x7)
                addi x3, x0, 1      # swap flag = 1
                
            no_swap:
                addi x4, x4, 1      # i++
                jal x0, sort_loop_inner
                
            check_swap:
                bne x3, x0, sort_loop_outer  # If swapped, repeat
                
            sort_done:
                
                # --- PHASE 2: MIXED EXTENSION HASHING ---
                # Iterate sorted array and mix values
                # x11 = Hash Accumulator
                # Uses: MUL (M), CLMUL (Zbc), BSET (Zbs)
                
                addi x11, x0, 0x123 # Initial seed
                addi x4, x0, 0      # index = 0
                
            hash_loop:
                bge x4, x2, finish
                
                # Load current value
                slli x6, x4, 2
                add  x7, x1, x6
                lw   x8, 0(x7)      # Load sorted value
                
                # 1. ADD: Mix in value
                add x11, x11, x8
                
                # 2. Zbc: Carry-less Multiply with a constant 
                # (Simulates CRC-like behavior)
                addi x12, x0, 0x1F
                clmul x11, x11, x12 
                
                # 3. Zbs: Set the 63rd bit to ensure we use 64-bit space
                addi x12, x0, 63
                bset x11, x11, x12
                
                # 4. M: Standard Multiply to scramble
                mul x11, x11, x8
                
                addi x4, x4, 1
                jal x0, hash_loop
                
            finish:
                la x20, hash_result
                sd x11, 0(x20)      # Store final hash
                ebreak
        ";

        AssembleAndLoad(source);
        
        // Execute (Estimate: Sort takes ~100-200 cycles, Hash takes ~50)
        int cycles = 0;
        while (!Machine.Halted && cycles < 2000)
        {
            Cycle();
            cycles++;
        }
        
        // --- VERIFICATION ---
        
        // 1. Verify Sort (1, 2, 3, 4, 5)
        uint baseAddr = (uint)Machine.Registers.Read(1);
        Assert.Equal(1u, Machine.Memory.ReadWord(baseAddr + 0));
        Assert.Equal(2u, Machine.Memory.ReadWord(baseAddr + 4));
        Assert.Equal(3u, Machine.Memory.ReadWord(baseAddr + 8));
        Assert.Equal(4u, Machine.Memory.ReadWord(baseAddr + 12));
        Assert.Equal(5u, Machine.Memory.ReadWord(baseAddr + 16));
        
        // 2. Verify Hash
        ulong hash = Machine.Registers.Read(11);
        _output.WriteLine($"Final Hash: {hash:X16}");
        
        Assert.True(hash != 0, "Hash should not be zero");
        // Check High Bit set by BSET
        Assert.True((hash & (1UL << 63)) != 0, "Zbs BSET failed to set bit 63");
    }

    [Fact]
    public void The_Hazard_Marathon()
    {
        // -----------------------------------------------------------
        // SCENARIO: A densely packed sequence forcing every pipeline hazard.
        // -----------------------------------------------------------
        
        var source = @"
            .text
            .globl _start
            _start:
                # Setup
                addi x1, x0, 10
                addi x2, x0, 20
                la   x10, var_mem
                sw   x1, 0(x10)     # Mem[var] = 10
                
                # -----------------------------------------------
                # 1. LOAD-USE STALL + MEM-TO-EX FORWARDING
                # -----------------------------------------------
                lw   x3, 0(x10)     # Load 10 into x3
                add  x4, x3, x1     # Use x3 immediately! (Stall 1 cyc, then Fwd)
                                    # x4 = 10 + 10 = 20
                
                # -----------------------------------------------
                # 2. WB-TO-EX FORWARDING
                # -----------------------------------------------
                # x4 is produced in Ex. 
                # Next instr uses it. This is Ex-to-Ex forwarding (fast path).
                add  x5, x4, x1     # x5 = 20 + 10 = 30
                
                # Insert bubble instructions to push x5 to WB stage
                nop
                nop 
                
                # Now x5 is in WB, x1 is in RegFile. 
                add  x6, x5, x1     # x6 = 30 + 10 = 40 (Read from WB/Reg)
                
                # -----------------------------------------------
                # 3. CONTROL HAZARD (BRANCH FLUSH)
                # -----------------------------------------------
                beq  x0, x0, skip   # Taken Branch
                addi x7, x0, 666    # Should be FLUSHED
                addi x7, x0, 666    # Should be FLUSHED
                
            skip:
                addi x7, x0, 1      # Target (x7 = 1)
                
                # -----------------------------------------------
                # 4. CONTROL HAZARD (JUMP FLUSH)
                # -----------------------------------------------
                jal  x0, finish
                addi x8, x0, 999    # Should be FLUSHED
                
            var_mem:
                .word 0
                
            finish:
                addi x8, x0, 100    # x8 = 100
                ebreak
        ";

        AssembleAndLoad(source);
        
        Cycle(50);
        
        _output.WriteLine("Register State Dump:");
        _output.WriteLine($"x3 (LW Result): {Machine.Registers.Read(3)} (Exp 10)");
        _output.WriteLine($"x4 (Load-Use):  {Machine.Registers.Read(4)} (Exp 20)");
        _output.WriteLine($"x5 (Ex-Fwd):    {Machine.Registers.Read(5)} (Exp 30)");
        _output.WriteLine($"x6 (WB-Read):   {Machine.Registers.Read(6)} (Exp 40)");
        _output.WriteLine($"x7 (Branch):    {Machine.Registers.Read(7)} (Exp 1)");
        _output.WriteLine($"x8 (Jump):      {Machine.Registers.Read(8)} (Exp 100)");

        AssertReg(3, 10ul);
        AssertReg(4, 20ul);
        AssertReg(5, 30ul);
        AssertReg(6, 40ul);
        AssertReg(7, 1ul);
        AssertReg(8, 100ul);
    }
}
