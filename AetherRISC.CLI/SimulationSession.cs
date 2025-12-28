using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI
{
    public class SimulationSession : IDisposable
    {
        public MachineState State { get; set; }
        public ISimulationLogger Logger { get; set; }
        
        // One of these will be not null
        public SimpleRunner? SimpleRunner { get; set; }
        public PipelinedRunner? PipelinedRunner { get; set; }
        
        public StringWriter OutputBuffer { get; set; } // Capture ECALL output

        public void Dispose()
        {
            if (Logger is IDisposable d) d.Dispose();
            OutputBuffer?.Dispose();
        }
    }
}
