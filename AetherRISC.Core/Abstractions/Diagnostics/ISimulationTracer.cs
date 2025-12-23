/*
 * Project:     AetherRISC
 * File:        ISimulationTracer.cs
 * Version:     1.0.0
 * Description: Defines the contract for the system's deterministic logging and tracing engine.
 * Used to capture pipeline states, memory access, and register changes.
 */

namespace AetherRISC.Core.Abstractions.Diagnostics;

public interface ISimulationTracer
{
    /// <summary>
    /// Logs a significant state change (e.g., Register Write, Memory Store).
    /// </summary>
    void LogStateChange(string component, string message);

    /// <summary>
    /// Logs a pipeline event (e.g., Stall, Flush, Hazard).
    /// </summary>
    void LogPipelineEvent(string stage, string eventName, string details);

    /// <summary>
    /// Logs an instruction execution trace.
    /// </summary>
    void LogInstruction(ulong pc, string disassembly);

    /// <summary>
    /// Logs a raw error or warning.
    /// </summary>
    void LogError(string component, string error);
}
