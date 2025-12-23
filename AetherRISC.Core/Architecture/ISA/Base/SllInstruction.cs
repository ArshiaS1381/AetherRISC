using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SllInstruction : Instruction {
        public override string Mnemonic => "SLL";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public SllInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            int shiftMask = (s.Config.XLEN == 32) ? 0x1F : 0x3F;
            int shamt = (int)s.Registers.Read(d.Rs2) & shiftMask;
            
            ulong val = s.Registers.Read(d.Rs1);
            ulong res = val << shamt;
            
            if (s.Config.XLEN == 32) res = (ulong)(uint)res;
            s.Registers.Write(d.Rd, res);
        }
    }
}
