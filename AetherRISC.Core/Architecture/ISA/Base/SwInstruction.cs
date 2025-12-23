using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.ISA.Base
{
    public class SwInstruction : Instruction
    {
        public override string Mnemonic => "SW";
        public override bool IsStore => true;
        public override int Rs1 { get; }
        public override int Rs2 { get; }
        public override int Imm { get; }
        public SwInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }

        public override void Execute(MachineState state, InstructionData data)
        {
            ulong addr = (ulong)((long)state.Registers.Read(data.Rs1) + (long)data.Immediate);
            uint val = (uint)state.Registers.Read(data.Rs2);
            state.Memory?.WriteWord((uint)addr, val);
        }
    }
}
