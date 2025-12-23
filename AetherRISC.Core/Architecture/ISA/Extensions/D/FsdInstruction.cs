using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FsdInstruction : Instruction {
        public override string Mnemonic => "FSD";
        public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
        
        public FsdInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
        
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             
             double val = s.FRegisters.ReadDouble(d.Rs2);
             // Use global::System to resolve BitConverter correctly
             ulong bits = global::System.BitConverter.DoubleToUInt64Bits(val);

             s.Memory!.WriteDouble((uint)addr, bits);
        }
    }
}
