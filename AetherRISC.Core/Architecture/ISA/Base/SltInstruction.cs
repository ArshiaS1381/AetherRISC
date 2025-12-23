using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SltInstruction : Instruction {
        public override string Mnemonic => "SLT";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public SltInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (long)s.Registers.Read(d.Rs1) < (long)s.Registers.Read(d.Rs2) ? 1ul : 0ul);
    }
}
