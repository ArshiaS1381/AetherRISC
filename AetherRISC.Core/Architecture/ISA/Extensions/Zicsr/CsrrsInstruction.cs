using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.Zicsr
{
    public class CsrrsInstruction : Instruction {
        public override string Mnemonic => "CSRRS";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public CsrrsInstruction(int rd, int rs1, int csr) { Rd = rd; Rs1 = rs1; Imm = csr; }
        public override void Execute(MachineState s, InstructionData d) {
            uint csr = (uint)d.Immediate;
            ulong t = s.Csr.Read(csr);
            ulong mask = s.Registers.Read(d.Rs1);
            if (d.Rs1 != 0) s.Csr.Write(csr, t | mask);
            s.Registers.Write(d.Rd, t);
        }
    }
}
