using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.Core.Architecture.Simulation.Runners
{
    public class SimpleRunner : ISimulationRunner
    {
        private readonly MachineState _state;
        private readonly InstructionDecoder _decoder;
        public PerformanceMetrics Metrics { get; } = new PerformanceMetrics();

        public SimpleRunner(MachineState state)
        {
            _state = state;
            _decoder = new InstructionDecoder();
            // Attach metrics to memory if using cache sim
            if (_state.Memory is AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy.CachedMemoryBus)
            {
                // In real implementation, ensure metrics reference is shared or injected
            }
        }

        public void Run(int maxCycles = -1)
        {
            int cycles = 0;
            while(!_state.Halted)
            {
                if (maxCycles != -1 && cycles >= maxCycles) break;
                StepInternal();
                cycles++;
            }
            Metrics.TotalCycles = (ulong)cycles;
        }

        public void Step(int count)
        {
            for(int i=0; i<count; i++) 
            {
                if(_state.Halted) break;
                StepInternal();
                Metrics.TotalCycles++;
            }
        }

        private void StepInternal()
        {
            // 1. Fetch
            uint raw = _state.Memory!.ReadWord((uint)_state.ProgramCounter);
            
            // 2. Decode
            var decoded = _decoder.DecodeFast(raw);
            if (decoded == null || decoded.Inst == null)
            {
                // Trap/Unknown
                _state.Halted = true;
                return;
            }

            InstructionData d = new InstructionData 
            { 
                Rd = decoded.Rd, 
                Rs1 = decoded.Rs1, 
                Rs2 = decoded.Rs2, 
                Imm = decoded.Imm,
                Immediate = (ulong)(long)decoded.Imm,
                PC = _state.ProgramCounter
            };

            // 3. Execute
            decoded.Inst.Execute(_state, d);
            
            Metrics.InstructionsRetired++;
            Metrics.IsaInstructionsRetired++;

            // Update PC (Instruction implementation might have jumped, so check if it was control flow)
            // Note: In Execute(), Jumps/Branches modify PC directly.
            // If it wasn't a taken branch/jump, advance PC.
            // This requires detecting if PC changed.
            // Simple approach: Assumes non-control insts don't touch PC.
            if (!decoded.IsBranch && !decoded.IsJump)
            {
                _state.ProgramCounter += 4; // Simplified, handle compressed later
            }
        }
    }
}
