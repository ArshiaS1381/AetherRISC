using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo
{
    // Fuses LUI/AUIPC + ADDI into a single 32-bit constant/address generation op.
    public class FusedComputationalInstruction : IInstruction
    {
        private readonly int _rd;
        private readonly long _finalResult;
        private readonly string _name;

        public string Mnemonic => _name;
        public int Rd => _rd;
        public int Rs1 => 0;
        public int Rs2 => 0;
        public int Imm => (int)_finalResult;

        public bool IsLoad => false;
        public bool IsStore => false;
        public bool IsBranch => false;
        public bool IsJump => false;
        public bool IsFloatRegWrite => false;

        public FusedComputationalInstruction(string name, int rd, long result)
        {
            _name = name;
            _rd = rd;
            _finalResult = result;
        }

        public void Execute(MachineState state, InstructionData data)
        {
            state.Registers.Write(_rd, (ulong)_finalResult);
        }

        public void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineMicroOp op)
        {
            op.AluResult = (ulong)_finalResult;
        }
    }
}
