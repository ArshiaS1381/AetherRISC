using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class AddiwInstruction : Instruction {
        public override string Mnemonic => "ADDIW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public AddiwInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
            long res = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
            s.Registers.Write(d.Rd, (ulong)(long)(int)res); // Truncate int32, then sext int64
        }
    }
}
