using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.A
{
    public class AmoXorInstruction : Instruction {
        public override string Mnemonic => IsWord ? "AMOXOR.W" : "AMOXOR.D";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public bool IsWord { get; }
        public AmoXorInstruction(int rd, int rs1, int rs2, bool isWord) { Rd = rd; Rs1 = rs1; Rs2 = rs2; IsWord = isWord; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            ulong val = s.Registers.Read(d.Rs2);
            ulong original;

            if (IsWord) {
                uint mem = s.Memory!.ReadWord((uint)addr);
                original = (ulong)(long)(int)mem;
                s.Memory.WriteWord((uint)addr, mem ^ (uint)val);
            } else {
                ulong mem = s.Memory!.ReadDouble((uint)addr);
                original = mem;
                s.Memory.WriteDouble((uint)addr, mem ^ val);
            }
            s.Registers.Write(d.Rd, original);
        }
    }
}
