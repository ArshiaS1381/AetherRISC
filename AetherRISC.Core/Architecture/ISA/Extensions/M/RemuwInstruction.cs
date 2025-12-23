using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class RemuwInstruction : Instruction {
        public override string Mnemonic => "REMUW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public RemuwInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            uint v1 = (uint)s.Registers.Read(d.Rs1);
            uint v2 = (uint)s.Registers.Read(d.Rs2);
            
            if (v2 == 0) s.Registers.Write(d.Rd, (ulong)(long)(int)v1);
            else s.Registers.Write(d.Rd, (ulong)(long)(int)(v1 % v2));
        }
    }
}
