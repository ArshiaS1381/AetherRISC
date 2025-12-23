using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class EbreakInstruction : Instruction {
        public override string Mnemonic => "EBREAK";
        public override void Execute(MachineState s, InstructionData d) { 
            // In a real CPU, this raises a Trap. For now, we can print or halt if DebugHost exists.
            s.Host?.PrintString("DEBUG: EBREAK Hit at PC " + d.PC);
        }
    }
}
