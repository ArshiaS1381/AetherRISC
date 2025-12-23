using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class JalrInstruction : Instruction {
        public override string Mnemonic => "JALR";
        public override bool IsJump => true;
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public JalrInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }

        public override void Execute(MachineState state, InstructionData data) {
            ulong rs1Val = state.Registers.Read(data.Rs1);
            state.Registers.Write(data.Rd, data.PC + 4);
            // JALR Target is Register Relative (Absolute)
            long offset = (long)data.Immediate;
            state.ProgramCounter = (ulong)((long)rs1Val + offset) & 0xFFFFFFFFFFFFFFFE;
        }
    }
}
