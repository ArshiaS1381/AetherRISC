using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Abstractions.Interfaces
{
    public interface ISimulationLogger
    {
        // Hints for the Runner to avoid expensive string formatting
        bool IsVerbose { get; } 

        void Initialize(string programName);
        void FinalizeSession();

        void BeginCycle(int cycle);
        void CompleteCycle();

        void Log(string component, string message);

        void LogStageFetch(ulong pc, uint raw);
        void LogStageDecode(ulong pc, uint raw, IInstruction inst);
        void LogStageExecute(ulong pc, uint raw, string info);
        void LogStageMemory(ulong pc, uint raw, string info);
        void LogStageWriteback(ulong pc, uint raw, int rdIndex, ulong value);

        void LogRegistersState(ulong[] registers);
    }
}
