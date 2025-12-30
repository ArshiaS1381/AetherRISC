using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AetherRISC.CLI
{
    public enum SimulationLogLevel { None, Simple, Verbose }

    public class CliConfig
    {
        [JsonPropertyName("programs_directory")] public string ProgramsDirectory { get; set; } = "programs";
        [JsonPropertyName("logs_directory")] public string LogsDirectory { get; set; } = "logs";
        [JsonPropertyName("memory_size")] public uint MemorySize { get; set; } = 1024 * 1024;
        [JsonPropertyName("execution_mode")] public string ExecutionMode { get; set; } = "simple";
        [JsonPropertyName("architecture")] public string Architecture { get; set; } = "rv64";
        [JsonPropertyName("stepping_mode")] public string SteppingMode { get; set; } = "auto";
        [JsonPropertyName("branch_predictor")] public string BranchPredictor { get; set; } = "static";
        [JsonPropertyName("show_realtime_metrics")] public bool ShowRealTimeMetrics { get; set; } = true;
        [JsonPropertyName("early_branch_resolution")] public bool EnableEarlyBranchResolution { get; set; } = true;
        [JsonPropertyName("predictor_init_value")] public int PredictorInitValue { get; set; } = 1;
        [JsonPropertyName("log_level")] [JsonConverter(typeof(JsonStringEnumConverter))] public SimulationLogLevel LogLevel { get; set; } = SimulationLogLevel.Simple;
        [JsonPropertyName("max_cycles")] public int MaxCycles { get; set; } = 100000;
        [JsonPropertyName("pipeline_width")] public int PipelineWidth { get; set; } = 1;
        [JsonPropertyName("enable_profiler")] public bool EnableProfiler { get; set; } = true;

        [JsonPropertyName("dynamic_fetching")] public bool AllowDynamicBranchFetching { get; set; } = false;
        [JsonPropertyName("cascaded_execution")] public bool AllowCascadedExecution { get; set; } = true;
        [JsonPropertyName("enable_ras")] public bool EnableReturnAddressStack { get; set; } = true;
        [JsonPropertyName("op_fusion")] public bool EnableMacroOpFusion { get; set; } = true;
        [JsonPropertyName("fetch_buffer_ratio")] public float FetchBufferRatio { get; set; } = 2.0f;
        [JsonPropertyName("tandem_verification")] public bool EnableTandemVerification { get; set; } = false;
        
        [JsonPropertyName("disabled_instructions")]
        public HashSet<string> DisabledInstructions { get; set; } = new HashSet<string>();
    }
}
