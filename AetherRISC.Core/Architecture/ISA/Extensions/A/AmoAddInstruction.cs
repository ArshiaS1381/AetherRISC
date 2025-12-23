using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.A
{
    public class AmoAddInstruction : Instruction {
        public override string Mnemonic => IsWord ? "AMOADD.W" : "AMOADD.D";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public bool IsWord { get; }

        public AmoAddInstruction(int rd, int rs1, int rs2, bool isWord) { Rd = rd; Rs1 = rs1; Rs2 = rs2; IsWord = isWord; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            ulong addVal = s.Registers.Read(d.Rs2); // Value to add
            
            ulong originalVal;

            if (IsWord) {
                // Read
                uint mem32 = s.Memory!.ReadWord((uint)addr);
                originalVal = (ulong)(long)(int)mem32; 
                
                // Modify (Add)
                uint res32 = mem32 + (uint)addVal;

                // Write
                s.Memory.WriteWord((uint)addr, res32);
            } else {
                // Read 64
                uint lo = s.Memory!.ReadWord((uint)addr);
                uint hi = s.Memory.ReadWord((uint)addr + 4);
                ulong mem64 = (ulong)lo | ((ulong)hi << 32);
                originalVal = mem64;

                // Modify
                ulong res64 = mem64 + addVal;

                // Write 64
                s.Memory.WriteWord((uint)addr, (uint)res64);
                s.Memory.WriteWord((uint)addr + 4, (uint)(res64 >> 32));
            }

            s.Registers.Write(d.Rd, originalVal);
        }
    }
}
