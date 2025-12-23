using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.A
{
    public class ScInstruction : Instruction {
        public override string Mnemonic => IsWord ? "SC.W" : "SC.D";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public bool IsWord { get; }

        public ScInstruction(int rd, int rs1, int rs2, bool isWord) { Rd = rd; Rs1 = rs1; Rs2 = rs2; IsWord = isWord; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            ulong valToWrite = s.Registers.Read(d.Rs2);

            // 1. Check Reservation
            bool success = false;
            
            // In a single-threaded emulator, the reservation is valid if the address matches.
            // (In real hardware, other cores could have invalidated it).
            if (s.LoadReservationAddress.HasValue && s.LoadReservationAddress.Value == addr)
            {
                success = true;
            }

            if (success)
            {
                // Perform Store
                if (IsWord) {
                    s.Memory!.WriteWord((uint)addr, (uint)valToWrite);
                } else {
                    s.Memory!.WriteWord((uint)addr, (uint)valToWrite);
                    s.Memory!.WriteWord((uint)addr + 4, (uint)(valToWrite >> 32));
                }
                
                // Success: Write 0 to Rd
                s.Registers.Write(d.Rd, 0);
            }
            else
            {
                // Failure: Write non-zero (1) to Rd, Memory UNTOUCHED
                s.Registers.Write(d.Rd, 1);
            }

            // SC always clears reservation
            s.LoadReservationAddress = null;
        }
    }
}
