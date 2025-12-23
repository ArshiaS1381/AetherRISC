using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SubInstruction : Instruction {
        public override string Mnemonic => "SUB";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public SubInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            ulong res = s.Registers.Read(d.Rs1) - s.Registers.Read(d.Rs2);
            if (s.Config.XLEN == 32) res = (ulong)(uint)res;
            s.Registers.Write(d.Rd, res);
        }
    }
}
