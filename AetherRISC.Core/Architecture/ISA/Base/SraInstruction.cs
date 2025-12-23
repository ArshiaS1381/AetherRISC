using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SraInstruction : Instruction {
        public override string Mnemonic => "SRA";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public SraInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)((long)s.Registers.Read(d.Rs1) >> (int)(s.Registers.Read(d.Rs2) & 0x3F)));
    }
}
