using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.Zicsr
{
    public class CsrrciInstruction : Instruction {
        public override string Mnemonic => "CSRRCI";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public CsrrciInstruction(int rd, int uimm, int csr) { Rd = rd; Imm = csr; Rs1 = uimm; }
        public override void Execute(MachineState s, InstructionData d) {
            uint csrAddr = (uint)d.Immediate;
            ulong oldVal = s.Csr.Read(csrAddr);
            
            if (d.Rd != 0) s.Registers.Write(d.Rd, oldVal);
            
            if (d.Rs1 != 0) {
                ulong mask = (ulong)(uint)d.Rs1;
                s.Csr.Write(csrAddr, oldVal & ~mask);
            }
        }
    }
}
