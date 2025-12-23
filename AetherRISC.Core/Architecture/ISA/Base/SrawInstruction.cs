using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SrawInstruction : Instruction {
        public override string Mnemonic => "SRAW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public SrawInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            int v1 = (int)s.Registers.Read(d.Rs1);
            int shamt = (int)s.Registers.Read(d.Rs2) & 0x1F;
            s.Registers.Write(d.Rd, (ulong)(long)(v1 >> shamt));
        }
    }
}
