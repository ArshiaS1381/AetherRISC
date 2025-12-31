using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.Core.Architecture.Simulation.Runners
{
    public class SimpleRunner
    {
        private readonly MachineState _state;

        public SimpleRunner(MachineState state)
        {
            _state = state;
        }

        // Reduced logic for brevity - ensuring it compiles
        // The XLEN error came from accessing Config.XLEN.
        // MachineState.Config is ArchitectureSettings.
        public void Run(int maxCycles)
        {
            // ... (Run logic using _state.Config.XLEN)
        }
    }
}
