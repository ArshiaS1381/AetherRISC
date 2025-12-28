using Xunit;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Architecture.Hardware.ISA.Utils;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Infrastructure;

public abstract class CpuTestFixture
{
    // "null!" tells the compiler: "Trust me, I will initialize this before use."
    protected MachineState Machine = null!;
    protected SimpleRunner Runner = null!;
    protected TestAssembler Assembler = null!;
    protected TestMemoryBus Memory = null!;

    protected void Init64() => Init(64);
    protected void Init32() => Init(32);

    private void Init(int xlen)
    {
        var config = new SystemConfig(xlen, resetVector: 0x0000);
        Machine = new MachineState(config);
        
        Memory = new TestMemoryBus(1024 * 1024); 
        Machine.Memory = Memory;

        Assembler = new TestAssembler();
        Runner = new SimpleRunner(Machine, new NullLogger());
    }

    protected void Run(int cycles)
    {
        var insts = Assembler.Assemble();
        uint addr = (uint)Machine.Config.ResetVector;

        foreach (var inst in insts)
        {
            uint raw = InstructionEncoder.Encode(inst);
            Memory.WriteWord(addr, raw);
            addr += 4;
        }

        Machine.ProgramCounter = Machine.Config.ResetVector;
        Runner.Run(cycles);
    }

    protected void AssertReg(int regIndex, ulong expected) => Assert.Equal(expected, Machine.Registers.Read(regIndex));
    protected void AssertReg(int regIndex, long expected) => Assert.Equal((ulong)expected, Machine.Registers.Read(regIndex));
    protected void AssertPC(ulong expected) => Assert.Equal(expected, Machine.ProgramCounter);
}
