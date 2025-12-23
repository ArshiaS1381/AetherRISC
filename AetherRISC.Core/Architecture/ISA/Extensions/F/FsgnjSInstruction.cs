using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FsgnjSInstruction : Instruction {
        public override string Mnemonic => "FsgnjS";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public FsgnjSInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        
        public override void Execute(MachineState s, InstructionData d) {
            float v1 = s.FRegisters.ReadSingle(d.Rs1);
            float v2 = s.FRegisters.ReadSingle(d.Rs2); // Source of sign
            
            // Extract raw bits
            uint b1 = BitConverter.SingleToUInt32Bits(v1);
            uint b2 = BitConverter.SingleToUInt32Bits(v2);
            uint signBit = (uint)(1UL << 31);
            
            // Logic: Inject Sign
            uint resBits = (b1 & ~signBit) | (b2 & signBit);
            
            float res = BitConverter.UInt32BitsToSingle(resBits);
            s.FRegisters.WriteSingle(d.Rd, res);
        }
    }
}
