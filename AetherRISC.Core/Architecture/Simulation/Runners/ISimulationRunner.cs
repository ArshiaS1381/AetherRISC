using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Simulation.Runners
{
    public interface ISimulationRunner
    {
        void Run(int maxCycles = -1);
        void Step(int cycleCount);
        PerformanceMetrics Metrics { get; }
    }
}
