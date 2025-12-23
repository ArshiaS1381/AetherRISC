/*
 * Project:     AetherRISC
 * File:        ConsoleTracer.cs
 * Version:     1.0.0
 * Description: Concrete implementation of ISimulationTracer for the CLI.
 */

using AetherRISC.Core.Abstractions.Diagnostics;

namespace AetherRISC.CLI;

public class ConsoleTracer : ISimulationTracer
{
    public void LogStateChange(string component, string message)
    {
        // Uncomment to see memory/register writes in the log
        // Visualizer.OutputLog.Add($"[STATE] {component}: {message}");
    }

    public void LogPipelineEvent(string stage, string eventName, string details)
    {
        if (eventName == "Flush")
             Visualizer.OutputLog.Add($"[PIPE] !!! FLUSH !!! {details}");
    }

    public void LogInstruction(ulong pc, string disassembly)
    {
        // Optional: Log every instruction execution
        // Visualizer.OutputLog.Add($"[EXEC] {pc:X}: {disassembly}");
    }

    public void LogError(string component, string error)
    {
        Visualizer.OutputLog.Add($"[ERROR] {component}: {error}");
    }
}
