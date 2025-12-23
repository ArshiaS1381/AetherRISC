using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class BltuInstruction : Instruction {
        public override string Mnemonic => "BLTU";
        public override bool IsBranch => true;
        public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
        public BltuInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
            // Unsigned Comparison
            if (s.Registers.Read(d.Rs1) < s.Registers.Read(d.Rs2)) 
                unchecked { s.ProgramCounter = (ulong)((long)d.PC + (long)d.Immediate); }
        }
    }
}
