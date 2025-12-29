using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI
{
    public class FileLogger : ISimulationLogger, IDisposable
    {
        private readonly Channel<string> _logChannel;
        private readonly Task _writeTask;
        private readonly CancellationTokenSource _cts;
        private readonly string _logPath;
        private bool _disposed;
        private readonly SimulationLogLevel _level;
        private readonly Stopwatch _timer;

        public bool IsVerbose => _level == SimulationLogLevel.Verbose;

        public FileLogger(string logFilePath, SimulationLogLevel level)
        {
            _logPath = logFilePath;
            _level = level;
            _timer = new Stopwatch();
            
            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            _logChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
            _cts = new CancellationTokenSource();
            _writeTask = Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        private async Task ProcessQueue()
        {
            using var fileStream = new FileStream(_logPath, FileMode.Create, FileAccess.Write, FileShare.Read, 65536, useAsync: true);
            using var writer = new StreamWriter(fileStream, Encoding.UTF8);
            try
            {
                while (await _logChannel.Reader.WaitToReadAsync(_cts.Token))
                {
                    while (_logChannel.Reader.TryRead(out var msg)) await writer.WriteLineAsync(msg);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Console.Error.WriteLine($"Logger Failed: {e.Message}"); }
            await writer.FlushAsync();
        }

        private void QueueEntry(string component, string message, bool isHeader)
        {
            if (_disposed) return;
            // Optimized: No DateTime.Now syscall. Use elapsed seconds from start.
            string output = isHeader 
                ? $"{Environment.NewLine}[{_timer.Elapsed.TotalSeconds:F4}s] [{component}] {message}" 
                : $"\t      [{component}] {message}";
            _logChannel.Writer.TryWrite(output);
        }

        public void Log(string component, string message) => QueueEntry(component, message, false);
        
        public void Initialize(string programName) 
        {
            _timer.Restart();
            QueueEntry("SYS", $"Simulation Start: {programName} (Level: {_level})", true);
        }

        public void FinalizeSession() 
        {
            _timer.Stop();
            QueueEntry("SYS", $"Session Finalized. Duration: {_timer.Elapsed.TotalSeconds:F4}s", true);
        }

        // --- Verbose Cycle Logging (Filtered) ---
        public void BeginCycle(int cycle) { if(IsVerbose) QueueEntry("CPU", $"=== Cycle {cycle} ===", true); }
        public void LogStageFetch(ulong pc, uint raw) { if(IsVerbose) QueueEntry("IF", $"PC: @{pc:X8} | Raw: 0x{raw:X8}", false); }
        public void LogStageDecode(ulong pc, uint raw, IInstruction inst) { if(IsVerbose) QueueEntry("ID", $"Decode {inst.Mnemonic}", false); }
        public void LogStageExecute(ulong pc, uint raw, string info) { if(IsVerbose) QueueEntry("EXE", info, false); }
        public void LogStageMemory(ulong pc, uint raw, string info) { if(IsVerbose) { if(!string.IsNullOrWhiteSpace(info)) QueueEntry("MEM", info, false); } }
        public void LogStageWriteback(ulong pc, uint raw, int rd, ulong val) { if(IsVerbose) QueueEntry("WB", $"x{rd,-2} <= 0x{val:X}", false); }
        
        public void LogRegistersState(ulong[] regs) { }
        public void CompleteCycle() { }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _logChannel.Writer.Complete();
            _cts.Cancel();
            try { _writeTask.Wait(1000); } catch { }
            _cts.Dispose();
        }
    }
}
