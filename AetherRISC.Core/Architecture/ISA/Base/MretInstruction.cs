using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Base
{
    public class MretInstruction : Instruction {
        public override string Mnemonic => "MRET";
        public override void Execute(MachineState s, InstructionData d) {
            // 1. Set PC to the value saved in MEPC CSR
            s.ProgramCounter = s.Csr.Read(0x341); // MEPC
            
            // 2. Privilege level logic would go here (returning to previous mode)
            // For now, we jump back to where we came from.
        }
    }
}
