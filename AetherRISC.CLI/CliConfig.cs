using System.Text.Json.Serialization;

namespace AetherRISC.CLI
{
    public class CliConfig
    {
        [JsonPropertyName("programs_directory")]
        public string ProgramsDirectory { get; set; } = "programs";

        [JsonPropertyName("logs_directory")]
        public string LogsDirectory { get; set; } = "logs";

        [JsonPropertyName("memory_size")]
        public uint MemorySize { get; set; } = 1024 * 1024;

        [JsonPropertyName("max_cycles")]
        public int MaxCycles { get; set; } = 100000;

        [JsonPropertyName("execution_mode")]
        public string ExecutionMode { get; set; } = "simple";

        [JsonPropertyName("architecture")]
        public string Architecture { get; set; } = "rv64";

        [JsonPropertyName("stepping_mode")]
        public string SteppingMode { get; set; } = "auto";
        
        [JsonPropertyName("enable_logging")]
        public bool EnableLogging { get; set; } = true;
    }
}
