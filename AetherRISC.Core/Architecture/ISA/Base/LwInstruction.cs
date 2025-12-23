using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.ISA.Base
{
    public class LwInstruction : Instruction
    {
        public override string Mnemonic => "LW";
        public override bool IsLoad => true;
        public override int Rd { get; }
        public override int Rs1 { get; }
        public override int Imm { get; }
        public LwInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }

        public override void Execute(MachineState state, InstructionData data)
        {
            ulong addr = (ulong)((long)state.Registers.Read(data.Rs1) + (long)data.Immediate);
            uint val = state.Memory?.ReadWord((uint)addr) ?? 0;
            state.Registers.Write(data.Rd, (ulong)(long)(int)val);
        }
    }
}
