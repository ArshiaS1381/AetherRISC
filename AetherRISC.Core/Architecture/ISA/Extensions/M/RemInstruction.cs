using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class RemInstruction : Instruction {
        public override string Mnemonic => "REM";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public RemInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            long v1, v2;
            
            if (s.Config.XLEN == 32) {
                v1 = (int)s.Registers.Read(d.Rs1);
                v2 = (int)s.Registers.Read(d.Rs2);
                
                if (v2 == 0) { s.Registers.Write(d.Rd, (ulong)(long)v1); return; }
                if (v1 == int.MinValue && v2 == -1) { s.Registers.Write(d.Rd, 0); return; }
            } else {
                v1 = (long)s.Registers.Read(d.Rs1);
                v2 = (long)s.Registers.Read(d.Rs2);
                
                if (v2 == 0) { s.Registers.Write(d.Rd, (ulong)v1); return; }
                if (v1 == long.MinValue && v2 == -1) { s.Registers.Write(d.Rd, 0); return; }
            }

            s.Registers.Write(d.Rd, (ulong)(v1 % v2));
        }
    }
}
