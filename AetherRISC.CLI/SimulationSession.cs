using System;
using System.IO;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI
{
    public class SimulationSession : IDisposable
    {
        public required MachineState State { get; set; }
        public required ISimulationLogger Logger { get; set; }
        public required StringWriter OutputBuffer { get; set; }

        public SimpleRunner? SimpleRunner { get; set; }
        public PipelinedRunner? PipelinedRunner { get; set; }
        
        public void Dispose()
        {
            if (Logger is IDisposable d) d.Dispose();
            OutputBuffer?.Dispose();
        }
    }
}
