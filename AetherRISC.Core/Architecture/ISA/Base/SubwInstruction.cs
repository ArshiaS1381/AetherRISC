using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SubwInstruction : Instruction {
        public override string Mnemonic => "SUBW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public SubwInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            long res = (long)s.Registers.Read(d.Rs1) - (long)s.Registers.Read(d.Rs2);
            s.Registers.Write(d.Rd, (ulong)(long)(int)res);
        }
    }
}
