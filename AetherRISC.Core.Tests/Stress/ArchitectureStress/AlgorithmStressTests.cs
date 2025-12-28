using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

// Simple Null Logger to avoid Moq dependency
public class NullLogger : ISimulationLogger
{
    public void Initialize(string programName) {}
    public void FinalizeSession() {}
    public void BeginCycle(int cycle) {}
    public void CompleteCycle() {}
    public void Log(string tag, string message) {}
    public void LogStageFetch(ulong pc, uint raw) {}
    public void LogStageDecode(ulong pc, uint raw, IInstruction inst) {}
    public void LogStageExecute(ulong pc, uint raw, string info) {}
    public void LogStageMemory(ulong pc, uint raw, string info) {}
    public void LogStageWriteback(ulong pc, uint raw, int rdIndex, ulong value) {}
    public void LogRegistersState(ulong[] registers) {}
}

public class AlgorithmStressTests
{
    [Fact]
    public void Pipelined_Memcpy_Loop_Stress()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(2048);
        var logger = new NullLogger();
        var runner = new PipelinedRunner(state, logger);

        // Setup: Copy 4 words from 0x100 to 0x200
        for(uint i=0; i<4; i++) state.Memory.WriteWord(0x100 + (i*4), 0xDEADBEEF + i);
        
        state.Registers.Write(10, 0x100); // a0 (src)
        state.Registers.Write(11, 0x200); // a1 (dst)
        state.Registers.Write(12, 4);     // a2 (count)

        var asm = new TestAssembler();
        asm.Add(pc => Inst.Lw(13, 10, 0), "loop");
        asm.Add(pc => Inst.Sw(11, 13, 0));
        asm.Add(pc => Inst.Addi(10, 10, 4));
        asm.Add(pc => Inst.Addi(11, 11, 4));
        asm.Add(pc => Inst.Addi(12, 12, -1));
        asm.Add(pc => Inst.Bne(12, 0, asm.To("loop", pc)));
        asm.Add(pc => Inst.Ebreak());

        var instructions = asm.Assemble();
        for (int i = 0; i < instructions.Count; i++)
        {
            state.Memory.WriteWord((uint)state.ProgramCounter + (uint)(i * 4), 
                AetherRISC.Core.Architecture.Hardware.ISA.Encoding.InstructionEncoder.Encode(instructions[i]));
        }

        runner.Run(200);

        for(uint i=0; i<4; i++) 
            Assert.Equal(0xDEADBEEF + i, state.Memory.ReadWord(0x200 + (i*4)));
    }
}
