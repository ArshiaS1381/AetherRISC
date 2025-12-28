using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.Core.Abstractions.Interfaces;

public interface IHostSystem
{
    void HandleEcall(MachineState state);
    void HandleBreak(MachineState state);
}
