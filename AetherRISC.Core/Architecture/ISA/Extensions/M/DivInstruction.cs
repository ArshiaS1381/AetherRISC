using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class DivInstruction : Instruction {
        public override string Mnemonic => "DIV";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public DivInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            long v1, v2;
            
            if (s.Config.XLEN == 32) {
                // Cast to int to interpret 32-bit patterns correctly
                v1 = (int)s.Registers.Read(d.Rs1);
                v2 = (int)s.Registers.Read(d.Rs2);
                
                if (v2 == 0) { s.Registers.Write(d.Rd, ulong.MaxValue); return; }
                
                // FIX: Use unchecked to allow casting negative constant (int.MinValue) to ulong
                if (v1 == int.MinValue && v2 == -1) { 
                    s.Registers.Write(d.Rd, unchecked((ulong)(long)int.MinValue)); 
                    return; 
                }
            } 
            else {
                v1 = (long)s.Registers.Read(d.Rs1);
                v2 = (long)s.Registers.Read(d.Rs2);
                
                if (v2 == 0) { s.Registers.Write(d.Rd, ulong.MaxValue); return; }
                
                // FIX: Use unchecked for 64-bit MinValue too
                if (v1 == long.MinValue && v2 == -1) { 
                    s.Registers.Write(d.Rd, unchecked((ulong)long.MinValue)); 
                    return; 
                }
            }

            // Normal Division (Integer division in C# truncates toward zero, which matches RISC-V)
            s.Registers.Write(d.Rd, unchecked((ulong)(v1 / v2)));
        }
    }
}
