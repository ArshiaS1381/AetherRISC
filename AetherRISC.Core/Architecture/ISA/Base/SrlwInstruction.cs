using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SrlwInstruction : Instruction {
        public override string Mnemonic => "SRLW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public SrlwInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            uint v1 = (uint)s.Registers.Read(d.Rs1);
            int shamt = (int)s.Registers.Read(d.Rs2) & 0x1F;
            s.Registers.Write(d.Rd, (ulong)(long)(int)(v1 >> shamt)); // Cast to int for sign-extend
        }
    }
}
