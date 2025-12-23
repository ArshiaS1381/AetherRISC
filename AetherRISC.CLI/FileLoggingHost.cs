/*
 * Project:     AetherRISC
 * File:        FileLoggingHost.cs
 * Version:     1.0.0
 * Description: Captures ECALL output to both Console and Trace Log.
 */

using System;
using System.IO;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI;

public class FileLoggingHost : ISystemCallHandler
{
    private readonly StreamWriter _writer;
    public bool IsRunning { get; private set; } = true;

    public FileLoggingHost(StreamWriter writer) 
    { 
        _writer = writer; 
    }

    public void PrintInt(long value) 
    {
        string msg = $"[OUTPUT] {value}";
        Console.WriteLine(msg);       // Screen
        _writer.WriteLine(msg);       // Log File
        _writer.Flush();
        Visualizer.OutputLog.Add(msg);
    }

    public void PrintString(string value) 
    {
        string msg = $"[OUTPUT] {value}";
        Console.WriteLine(msg);
        _writer.WriteLine(msg);
        _writer.Flush();
        Visualizer.OutputLog.Add(msg);
    }
    
    public void Exit(int code)
    {
        string msg = $"[SYSTEM HALT] Exit Code {code}";
        Console.WriteLine(msg);
        _writer.WriteLine(msg);
        _writer.Flush();
        Visualizer.OutputLog.Add(msg);
        IsRunning = false; 
    }
}
