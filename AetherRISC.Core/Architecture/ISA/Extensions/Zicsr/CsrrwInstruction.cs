using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.Zicsr
{
    public class CsrrwInstruction : Instruction {
        public override string Mnemonic => "CSRRW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public CsrrwInstruction(int rd, int rs1, int csr) { Rd = rd; Rs1 = rs1; Imm = csr; }
        public override void Execute(MachineState s, InstructionData d) {
            uint csr = (uint)d.Immediate;
            ulong t = s.Csr.Read(csr);
            s.Csr.Write(csr, s.Registers.Read(d.Rs1));
            s.Registers.Write(d.Rd, t);
        }
    }
}
