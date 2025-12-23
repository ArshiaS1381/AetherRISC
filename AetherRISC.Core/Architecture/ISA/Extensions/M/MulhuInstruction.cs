using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class MulhuInstruction : Instruction {
        public override string Mnemonic => "MULHU";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public MulhuInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            if (s.Config.XLEN == 32)
            {
                uint v1 = (uint)s.Registers.Read(d.Rs1);
                uint v2 = (uint)s.Registers.Read(d.Rs2);
                ulong res = (ulong)v1 * (ulong)v2;
                s.Registers.Write(d.Rd, res >> 32);
            }
            else
            {
                ulong v1 = s.Registers.Read(d.Rs1);
                ulong v2 = s.Registers.Read(d.Rs2);
                UInt128 res = (UInt128)v1 * (UInt128)v2;
                s.Registers.Write(d.Rd, (ulong)(res >> 64));
            }
        }
    }
}
