using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class SyscallStressTests
{
    [Fact]
    public void Linux_Write_Syscall_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var handler = new MultiOSHandler { Kind = OSKind.Linux };
        state.Host = handler;

        string code = @"
            .data
            msg: .asciz ""Linux!""
            .text
            li a0, 1
            la a1, msg
            li a2, 6
            li a7, 64
            ecall
            mv t0, a0
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(300);

        // After write syscall, a0 should contain bytes written (6)
        // We copy to t0 to preserve across potential pipeline issues
        Assert.Equal(6ul, state.Registers.Read(5)); // t0
    }

    [Fact]
    public void RARS_PrintInt_Syscall_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var handler = new MultiOSHandler { Kind = OSKind.RARS };
        state.Host = handler;

        string code = @"
            .text
            li a0, 42
            li a7, 1
            ecall
            mv t0, a0
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(150);
        
        // print_int doesn't modify a0, so t0 should still be 42
        Assert.Equal(42ul, state.Registers.Read(5));
    }

    [Fact]
    public void RARS_Sbrk_Syscall_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var handler = new MultiOSHandler { Kind = OSKind.RARS };
        state.Host = handler;

        string code = @"
            .text
            li a0, 1024
            li a7, 9
            ecall
            mv t0, a0
            li a0, 512
            li a7, 9
            ecall
            mv t1, a0
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(250);
        
        ulong firstAlloc = state.Registers.Read(5);  // t0
        ulong secondAlloc = state.Registers.Read(6); // t1
        
        // Second allocation should be 1024 bytes after first
        Assert.Equal(1024ul, secondAlloc - firstAlloc);
    }

    [Fact]
    public void RARS_Time_Syscall_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var handler = new MultiOSHandler { Kind = OSKind.RARS };
        state.Host = handler;

        string code = @"
            .text
            li a7, 30
            ecall
            mv t0, a0
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(150);
        
        ulong timestamp = state.Registers.Read(5); // t0
        
        // Should be a reasonable Unix timestamp (after year 2020)
        Assert.True(timestamp > 1577836800000, $"Timestamp {timestamp} should be > 1577836800000");
    }

    [Fact]
    public void Linux_Brk_Syscall_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var handler = new MultiOSHandler { Kind = OSKind.Linux };
        state.Host = handler;

        string code = @"
            .text
            li a0, 0
            li a7, 214
            ecall
            mv t0, a0
            li a0, 0x10050000
            li a7, 214
            ecall
            mv t1, a0
            li a0, 0
            li a7, 214
            ecall
            mv t2, a0
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(350);
        
        ulong initialBrk = state.Registers.Read(5);  // t0 - initial heap
        ulong newBrk = state.Registers.Read(6);      // t1 - after setting
        ulong currentBrk = state.Registers.Read(7);  // t2 - query again
        
        Assert.Equal(0x10050000ul, newBrk);
        Assert.Equal(0x10050000ul, currentBrk);
    }

    [Fact]
    public void RARS_RandomInt_Syscall_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var handler = new MultiOSHandler { Kind = OSKind.RARS };
        state.Host = handler;

        string code = @"
            .text
            li a0, 0
            li a1, 12345
            li a7, 40
            ecall
            li a0, 0
            li a1, 100
            li a7, 42
            ecall
            mv t0, a0
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(250);
        
        ulong randomRange = state.Registers.Read(5);   // t0
        
        // Random in range should be [0, 100)
        Assert.True(randomRange < 100, $"Random value {randomRange} should be < 100");
    }

    [Fact]
    public void Multiple_Syscalls_Sequence_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        var handler = new MultiOSHandler { Kind = OSKind.RARS };
        state.Host = handler;

        string code = @"
            .text
            li a7, 30
            ecall
            mv s0, a0
            li a0, 256
            li a7, 9
            ecall
            mv s1, a0
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(250);
        
        ulong timestamp = state.Registers.Read(8);   // s0
        ulong heapAddr = state.Registers.Read(9);    // s1
        
        Assert.True(timestamp > 0, $"Timestamp should be > 0, got {timestamp}");
        Assert.True(heapAddr >= 0x10040000, $"Heap addr {heapAddr:X} should be >= 0x10040000");
    }
}
