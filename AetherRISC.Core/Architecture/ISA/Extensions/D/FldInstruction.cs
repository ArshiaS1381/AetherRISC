using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FldInstruction : Instruction {
        public override string Mnemonic => "FLD";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        
        public FldInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             ulong val = s.Memory!.ReadDouble((uint)addr);
             
             // Use global::System to resolve BitConverter correctly
             s.FRegisters.WriteDouble(d.Rd, global::System.BitConverter.UInt64BitsToDouble(val));
        }
    }
}
