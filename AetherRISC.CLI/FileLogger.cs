using System;
using System.IO;
using System.Text;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI
{
    public class FileLogger : ISimulationLogger, IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly bool _consoleEcho;

        public FileLogger(string logFilePath, bool consoleEcho = false)
        {
            _consoleEcho = consoleEcho;
            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(logFilePath, append: false) { AutoFlush = true };
        }

        /// <summary>
        /// Helper to handle the "Only print time on header" rule and spacing.
        /// </summary>
        private void WriteLogEntry(string component, string message, bool isHeader)
        {
            string output;
            if (isHeader)
            {
                // Header: Time included, NewLine before for spacing
                output = $"{Environment.NewLine}[{DateTime.Now:HH:mm:ss.fff}] [{component}] {message}";
            }
            else
            {
                // Body: Indented, No Time, spacious alignment
                output = $"\t      [{component}] {message}";
            }

            _writer.WriteLine(output);
            if (_consoleEcho) Console.WriteLine(output);
        }

        // --- ISimulationLogger Implementation ---

        // ERROR FIX: Re-implemented public Log(string, string) to satisfy interface
        public void Log(string component, string message)
        {
            // Default generic log calls are treated as "Body" (no timestamp)
            WriteLogEntry(component, message, isHeader: false);
        }

        public void Initialize(string programName) 
        {
            WriteLogEntry("SYS", $"Initializing simulation for {programName}", isHeader: true);
        }
        
        public void BeginCycle(int cycle) 
        {
            // Only this one gets the Timestamp and Header treatment
            WriteLogEntry("CPU", $"==================== Cycle {cycle} ====================", isHeader: true);
        }

        public void LogStageFetch(ulong pc, uint raw)
        {
            // Format: 0000_0000_0000_0000_0000_0000_0000_0000
            string bin = Convert.ToString(raw, 2).PadLeft(32, '0');
            string prettyBin = $"{bin.Substring(0, 8)}_{bin.Substring(8, 8)}_{bin.Substring(16, 8)}_{bin.Substring(24, 8)}";

            // Format: PC as Address (@00000000), Raw as Hex (0x) and Bin (0b)
            string msg = $"PC: @{pc:X8} | Raw: 0x{raw:X8} (0b{prettyBin})";
            WriteLogEntry("IF", msg, isHeader: false);
        }

        public void LogStageDecode(ulong pc, uint raw, IInstruction instruction)
        {
            // Align fields for readability
            var details = $"(Rd: {instruction.Rd}, Rs1: {instruction.Rs1}, Imm: 0x{instruction.Imm:X})";
            WriteLogEntry("ID", $"Decode {instruction.Mnemonic,-6} | {details}", isHeader: false);
        }

        public void LogStageExecute(ulong pc, uint raw, string info)
        {
            WriteLogEntry("EXE", $"Exec | {info}", isHeader: false);
        }

        public void LogStageMemory(ulong pc, uint raw, string info)
        {
            // Even if idle, we print it to maintain the visual "Lane" for MEM
            if (string.IsNullOrWhiteSpace(info))
            {
                WriteLogEntry("MEM", "Idle", isHeader: false);
            }
            else
            {
                WriteLogEntry("MEM", info, isHeader: false);
            }
        }

        public void LogStageWriteback(ulong pc, uint raw, int rd, ulong val)
        {
            WriteLogEntry("WB", $"x{rd,-2} <= 0x{val:X}", isHeader: false);
        }
        
        public void LogRegistersState(ulong[] regs)
        {
            // Interface requirement - blank is fine if you don't want register dumps every cycle
        }

        public void CompleteCycle() { }
        
        public void FinalizeSession() => WriteLogEntry("SYS", "Session Finalized", isHeader: true);

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
