using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V
{
    // VSETVLI: Set Vector Length and Type
    [RiscvInstruction("VSETVLI", InstructionSet.RV64I, RiscvEncodingType.I, 0x57, Funct3 = 7)]
    public class VsetvliInstruction : ITypeInstruction
    {
        public VsetvliInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

        public override void Execute(MachineState s, InstructionData d)
        {
            // FIXED: Using VRegisters instead of FRegisters
            int avl = (d.Rs1 == 0) ? s.VRegisters.Vl : (int)s.Registers.Read(d.Rs1); 
            if (d.Rs1 != 0) avl = (int)s.Registers.Read(d.Rs1); 

            s.VRegisters.UpdateVtype((ulong)d.Imm, avl);
            s.Registers.Write(d.Rd, (ulong)s.VRegisters.Vl);
        }

        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op)
        {
            // FIXED: Using VRegisters instead of FRegisters
            int avl = (Rs1 == 0) ? s.VRegisters.Vl : (int)r1; 
            s.VRegisters.UpdateVtype((ulong)Imm, avl); 
            op.AluResult = (ulong)s.VRegisters.Vl;
        }
    }
}
