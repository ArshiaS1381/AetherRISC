using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Tests;

public sealed class TestSimulationLogger : ISimulationLogger
{
    public void Initialize(string sessionName) { }
    public void FinalizeSession() { }

    public void BeginCycle(int cycle) { }
    public void CompleteCycle() { }

    public void Log(string tag, string message) { }

    public void LogStageFetch(ulong pc, uint rawInstruction) { }

    public void LogStageDecode(
        ulong pc,
        uint rawInstruction,
        AetherRISC.Core.Abstractions.Interfaces.IInstruction instruction
    )
    { }

    public void LogStageExecute(ulong pc, uint rawInstruction, string executionInfo) { }

    public void LogStageMemory(ulong pc, uint rawInstruction, string memoryInfo) { }

    public void LogStageWriteback(
        ulong pc,
        uint rawInstruction,
        int rd,
        ulong result
    )
    { }

    public void LogRegistersState(ulong[] registers) { }
}
