using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class AuipcInstruction : Instruction {
        public override string Mnemonic => "AUIPC";
        public override int Rd { get; } public override int Imm { get; }
        public AuipcInstruction(int rd, int imm) { Rd = rd; Imm = imm; }

        public override void Execute(MachineState state, InstructionData data) {
            // AUIPC = PC + Upper Immediate
            ulong res = (ulong)((long)data.PC + (long)data.Immediate);
            state.Registers.Write(data.Rd, res);
        }
    }
}
