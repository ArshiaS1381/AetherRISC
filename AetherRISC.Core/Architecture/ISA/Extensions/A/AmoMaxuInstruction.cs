using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.A
{
    public class AmoMaxuInstruction : Instruction {
        public override string Mnemonic => IsWord ? "AMOMAXU.W" : "AMOMAXU.D";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public bool IsWord { get; }
        public AmoMaxuInstruction(int rd, int rs1, int rs2, bool isWord) { Rd = rd; Rs1 = rs1; Rs2 = rs2; IsWord = isWord; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            ulong val = s.Registers.Read(d.Rs2);
            ulong original;

            if (IsWord) {
                uint memVal = s.Memory!.ReadWord((uint)addr); // Unsigned
                uint regVal = (uint)val;
                original = (ulong)(long)(int)memVal; // Sign-extend result
                s.Memory.WriteWord((uint)addr, Math.Max(memVal, regVal));
            } else {
                ulong memVal = s.Memory!.ReadDouble((uint)addr); // Unsigned
                original = memVal;
                s.Memory.WriteDouble((uint)addr, Math.Max(memVal, val));
            }
            s.Registers.Write(d.Rd, original);
        }
    }
}
