using System;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI;

public class ConsoleHost : ISystemCallHandler
{
    public bool IsRunning { get; private set; } = true;

    public void PrintInt(long value) => Visualizer.OutputLog.Add($"[OUTPUT] {value}");
    public void PrintString(string value) => Visualizer.OutputLog.Add($"[OUTPUT] {value}");
    
    public void Exit(int code)
    {
        Visualizer.OutputLog.Add($"[SYSTEM HALT] Exit Code {code}");
        IsRunning = false; // Stop the simulator
    }
}
