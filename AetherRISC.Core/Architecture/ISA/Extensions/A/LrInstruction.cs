using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.A
{
    public class LrInstruction : Instruction {
        public override string Mnemonic => IsWord ? "LR.W" : "LR.D";
        public override int Rd { get; } public override int Rs1 { get; } 
        public bool IsWord { get; } // True for .W, False for .D

        public LrInstruction(int rd, int rs1, bool isWord) { Rd = rd; Rs1 = rs1; IsWord = isWord; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            
            // 1. Perform the Load
            if (IsWord) {
                // LR.W loads 32-bits, sign-extends to 64
                int val = (int)s.Memory!.ReadWord((uint)addr);
                s.Registers.Write(d.Rd, (ulong)(long)val);
            } else {
                // LR.D loads 64-bits
                ulong val = s.Memory!.ReadDouble((uint)addr); // Assuming Memory supports ReadDouble, usually 2x ReadWord
                // Fallback if no ReadDouble: Read 2 words
                if (s.Config.XLEN == 32) { /* RV32 doesn't have LR.D usually, but valid in RV64 */ }
                
                // For emulation simplicity, assuming generic 64-bit load support or manual fetch:
                // We will implement a helper below if needed, but for now assuming byte access:
                // Actually, let's just stick to ReadWord for W and assuming ReadWord*2 logic exists or we add it.
                // Let's implement manual 64-bit read here to be safe:
                uint lo = s.Memory.ReadWord((uint)addr);
                uint hi = s.Memory.ReadWord((uint)addr + 4);
                s.Registers.Write(d.Rd, (ulong)lo | ((ulong)hi << 32));
            }

            // 2. Register Reservation
            s.LoadReservationAddress = addr;
        }
    }
}
