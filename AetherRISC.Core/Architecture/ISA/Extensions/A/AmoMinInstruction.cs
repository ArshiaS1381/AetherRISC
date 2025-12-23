using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.A
{
    public class AmoMinInstruction : Instruction {
        public override string Mnemonic => IsWord ? "AMOMIN.W" : "AMOMIN.D";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public bool IsWord { get; }
        public AmoMinInstruction(int rd, int rs1, int rs2, bool isWord) { Rd = rd; Rs1 = rs1; Rs2 = rs2; IsWord = isWord; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            ulong val = s.Registers.Read(d.Rs2);
            ulong original;

            if (IsWord) {
                int memVal = (int)s.Memory!.ReadWord((uint)addr);
                int regVal = (int)val;
                original = (ulong)(long)memVal;
                s.Memory.WriteWord((uint)addr, (uint)Math.Min(memVal, regVal));
            } else {
                long memVal = (long)s.Memory!.ReadDouble((uint)addr);
                long regVal = (long)val;
                original = (ulong)memVal;
                s.Memory.WriteDouble((uint)addr, (ulong)Math.Min(memVal, regVal));
            }
            s.Registers.Write(d.Rd, original);
        }
    }
}
