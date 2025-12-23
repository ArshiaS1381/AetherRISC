using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.ISA.Base
{
    public class LuiInstruction : Instruction
    {
        public override string Mnemonic => "LUI";
        public override int Rd { get; }
        public override int Imm { get; }
        public LuiInstruction(int rd, int imm) { Rd = rd; Imm = imm; }

        public override void Execute(MachineState state, InstructionData data)
        {
            state.Registers.Write(data.Rd, data.Immediate);
        }
    }
}
