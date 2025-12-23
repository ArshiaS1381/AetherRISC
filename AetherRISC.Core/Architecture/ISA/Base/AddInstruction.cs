using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class AddInstruction : Instruction {
        public override string Mnemonic => "ADD";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public AddInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            ulong res = s.Registers.Read(d.Rs1) + s.Registers.Read(d.Rs2);
            if (s.Config.XLEN == 32) res = (ulong)(uint)res; // Truncate to 32 bits
            s.Registers.Write(d.Rd, res);
        }
    }
}
