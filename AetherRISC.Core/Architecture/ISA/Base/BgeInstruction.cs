using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class BgeInstruction : Instruction {
        public override string Mnemonic => "BGE";
        public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; } public int Funct3 { get; }

        public BgeInstruction(int rs1, int rs2, int imm) : this(rs1, rs2, imm, 5) { }
        public BgeInstruction(int rs1, int rs2, int imm, int funct3) { Rs1 = rs1; Rs2 = rs2; Imm = imm; Funct3 = funct3; }
        
        public override void Execute(MachineState s, InstructionData d) {
             long v1 = (long)s.Registers.Read(d.Rs1);
             long v2 = (long)s.Registers.Read(d.Rs2);
             if (v1 >= v2) s.ProgramCounter = (ulong)((long)d.PC + (long)d.Immediate - 4);
        }
    }
}
