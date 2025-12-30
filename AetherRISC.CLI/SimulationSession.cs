using System;
using System.IO;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI
{
    public class MismatchInfo
    {
        public string Reason { get; set; } = "";
        public string Location { get; set; } = "";
        public string Expected { get; set; } = "";
        public string Actual { get; set; } = "";
    }

    public class SimulationSession : IDisposable
    {
        public required string ProgramName { get; set; } // NEW
        public required MachineState State { get; set; }
        public required ISimulationLogger Logger { get; set; }
        public required TextWriter OutputBuffer { get; set; }

        public SimpleRunner? SimpleRunner { get; set; }
        public PipelinedRunner? PipelinedRunner { get; set; }

        public SimpleRunner? ShadowRunner { get; set; }
        public MachineState? ShadowState { get; set; }
        public TextWriter? ShadowOutputBuffer { get; set; } 
        
        public bool VerificationFailed { get; set; } = false;
        public bool VerificationPassed { get; set; } = false;
        public MismatchInfo? FailureDetails { get; set; }
        
        public void Dispose()
        {
            if (Logger is IDisposable d) d.Dispose();
            OutputBuffer?.Dispose();
            ShadowOutputBuffer?.Dispose();
        }
    }
}
