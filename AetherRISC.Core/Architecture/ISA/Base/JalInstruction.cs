using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class JalInstruction : Instruction {
        public override string Mnemonic => "JAL";
        public override bool IsJump => true;
        public override int Rd { get; } public override int Imm { get; }
        public JalInstruction(int rd, int imm) { Rd = rd; Imm = imm; }

        public override void Execute(MachineState state, InstructionData data) {
            // Correct Return Address: Instruction PC + 4
            state.Registers.Write(data.Rd, data.PC + 4); 
            // Correct Target: Instruction PC + Offset
            long offset = (long)data.Immediate;
            unchecked { state.ProgramCounter = (ulong)((long)data.PC + offset); }
        }
    }
}
