using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.A
{
    public class AmoSwapInstruction : Instruction {
        public override string Mnemonic => IsWord ? "AMOSWAP.W" : "AMOSWAP.D";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public bool IsWord { get; }

        public AmoSwapInstruction(int rd, int rs1, int rs2, bool isWord) { Rd = rd; Rs1 = rs1; Rs2 = rs2; IsWord = isWord; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            ulong srcVal = s.Registers.Read(d.Rs2); // Value to write
            
            ulong loadedVal;

            if (IsWord) {
                // 1. Read Old Value
                uint old32 = s.Memory!.ReadWord((uint)addr);
                loadedVal = (ulong)(long)(int)old32; // Sign extend result
                
                // 2. Write New Value
                s.Memory.WriteWord((uint)addr, (uint)srcVal);
            } else {
                // 64-bit Load
                uint lo = s.Memory!.ReadWord((uint)addr);
                uint hi = s.Memory.ReadWord((uint)addr + 4);
                loadedVal = (ulong)lo | ((ulong)hi << 32);

                // 64-bit Store
                s.Memory.WriteWord((uint)addr, (uint)srcVal);
                s.Memory.WriteWord((uint)addr + 4, (uint)(srcVal >> 32));
            }

            // 3. Write Old Value to Rd
            s.Registers.Write(d.Rd, loadedVal);
        }
    }
}
