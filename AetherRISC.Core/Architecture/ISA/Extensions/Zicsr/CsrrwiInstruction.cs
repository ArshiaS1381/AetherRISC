using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.Zicsr
{
    public class CsrrwiInstruction : Instruction {
        public override string Mnemonic => "CSRRWI";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public CsrrwiInstruction(int rd, int uimm, int csr) { Rd = rd; Rs1 = uimm; Imm = csr; }
        public override void Execute(MachineState s, InstructionData d) {
            uint csr = (uint)d.Immediate;
            ulong t = s.Csr.Read(csr);
            s.Csr.Write(csr, (ulong)d.Rs1);
            s.Registers.Write(d.Rd, t);
        }
    }
}
