using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FsgnjDInstruction : Instruction {
        public override string Mnemonic => "FsgnjD";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public FsgnjDInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        
        public override void Execute(MachineState s, InstructionData d) {
            double v1 = s.FRegisters.ReadDouble(d.Rs1);
            double v2 = s.FRegisters.ReadDouble(d.Rs2); // Source of sign
            
            // Extract raw bits
            ulong b1 = BitConverter.DoubleToUInt64Bits(v1);
            ulong b2 = BitConverter.DoubleToUInt64Bits(v2);
            ulong signBit = (ulong)(1UL << 63);
            
            // Logic: Inject Sign
            ulong resBits = (b1 & ~signBit) | (b2 & signBit);
            
            double res = BitConverter.UInt64BitsToDouble(resBits);
            s.FRegisters.WriteDouble(d.Rd, res);
        }
    }
}
