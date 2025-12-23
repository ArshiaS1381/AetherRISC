using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture; 

namespace AetherRISC.Core.Architecture.ISA.Base
{
    public abstract class Instruction : IInstruction
    {
        public abstract string Mnemonic { get; }
        
        public virtual int Rd => 0;
        public virtual int Rs1 => 0;
        public virtual int Rs2 => 0;
        public virtual int Imm => 0;

        public virtual bool IsLoad => false;
        public virtual bool IsStore => false;
        public virtual bool IsBranch => false;
        public virtual bool IsJump => false;

        public abstract void Execute(MachineState state, InstructionData data);
    }
}
