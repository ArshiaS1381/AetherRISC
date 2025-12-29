using AetherRISC.Core.Assembler;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI
{
    public class ProgramLoader
    {
        public CliConfig Config { get; private set; } = new CliConfig();
        private readonly string _baseDir;
        private readonly string _configPath;

        public ProgramLoader(string configFileName)
        {
            var possiblePaths = new[]
            {
                configFileName,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName),
                Path.Combine(Directory.GetCurrentDirectory(), "AetherRISC.CLI", configFileName),
                Path.Combine("..", "AetherRISC.CLI", configFileName)
            };

            string? finalPath = possiblePaths.FirstOrDefault(File.Exists);
            if (finalPath == null) throw new FileNotFoundException($"Config '{configFileName}' not found.");

            _configPath = Path.GetFullPath(finalPath);
            _baseDir = Path.GetDirectoryName(_configPath)!;
            LoadConfig();
        }

        private void LoadConfig()
        {
            var json = File.ReadAllText(_configPath);
            Config = JsonSerializer.Deserialize<CliConfig>(json) ?? new CliConfig();
        }

        public void SaveConfig()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_configPath, JsonSerializer.Serialize(Config, options));
        }

        public string[] GetAvailablePrograms()
        {
            var progDir = Path.Combine(_baseDir, Config.ProgramsDirectory);
            if (!Directory.Exists(progDir)) Directory.CreateDirectory(progDir);
            return Directory.GetFiles(progDir, "*.*").Where(s => s.EndsWith(".asm") || s.EndsWith(".s") || s.EndsWith(".txt")).ToArray();
        }

        public SimulationSession PrepareSession(string filePath)
        {
            var sysConfig = Config.Architecture.Equals("rv32", StringComparison.OrdinalIgnoreCase) 
                ? SystemConfig.Rv32() : SystemConfig.Rv64();
            
            var state = new MachineState(sysConfig);
            state.Memory = new SystemBus(Config.MemorySize);
            
            var outBuffer = new StringWriter();
            var host = new MultiOSHandler { Kind = OSKind.RARS, Silent = false, Output = outBuffer };
            state.Host = host;

            string sourceCode = File.ReadAllText(filePath);
            var assembler = new SourceAssembler(sourceCode) { TextBase = 0 };
            assembler.Assemble(state); 
            
            ISimulationLogger logger;
            if (Config.LogLevel != SimulationLogLevel.None)
            {
                var logDir = Path.Combine(_baseDir, Config.LogsDirectory);
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                
                var fileLogger = new FileLogger(logPath, Config.LogLevel);
                fileLogger.Initialize(Path.GetFileName(filePath));
                logger = fileLogger;
            }
            else
            {
                logger = new NullLogger();
            }

            var session = new SimulationSession
            {
                State = state,
                Logger = logger,
                OutputBuffer = outBuffer
            };

            if (Config.ExecutionMode.Equals("pipeline", StringComparison.OrdinalIgnoreCase))
                // PASSING THE CONFIG VALUE HERE
                session.PipelinedRunner = new PipelinedRunner(state, logger, Config.BranchPredictor);
            else
                session.SimpleRunner = new SimpleRunner(state, logger);

            return session;
        }
    }
}
