using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI
{
    public class NullLogger : ISimulationLogger
    {
        public bool IsVerbose => false; // Optimization hint: Always false

        public void Initialize(string programName) { }
        public void FinalizeSession() { }
        public void BeginCycle(int cycle) { }
        public void CompleteCycle() { }
        public void Log(string component, string message) { }
        public void LogStageFetch(ulong pc, uint raw) { }
        public void LogStageDecode(ulong pc, uint raw, IInstruction inst) { }
        public void LogStageExecute(ulong pc, uint raw, string info) { }
        public void LogStageMemory(ulong pc, uint raw, string info) { }
        public void LogStageWriteback(ulong pc, uint raw, int rdIndex, ulong value) { }
        public void LogRegistersState(ulong[] registers) { }
    }
}
