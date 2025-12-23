using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class OrInstruction : Instruction {
        public override string Mnemonic => "OR";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public OrInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | s.Registers.Read(d.Rs2));
    }
}
