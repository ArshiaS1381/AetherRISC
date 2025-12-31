using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64V
{
    // OPIVV (Vector-Vector Arithmetic)
    [RiscvInstruction("VADD.VV", InstructionSet.RV64I, RiscvEncodingType.R, 0x57, Funct3 = 0, Funct6 = 0x00)]
    public class VaddVvInstruction : RTypeInstruction
    {
        public VaddVvInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

        public override void Execute(MachineState s, InstructionData d)
        {
            int vl = s.VRegisters.Vl;
            int sew = s.VRegisters.SewBytes;
            
            var vs1 = s.VRegisters.GetRaw(d.Rs1);
            var vs2 = s.VRegisters.GetRaw(d.Rs2);
            var vd = new byte[s.VRegisters.VLenBytes];

            // Perform element-wise addition based on SEW
            // Note: Simplification for simulator speed -> treats as byte arrays
            // For proper SEW support, we'd need switch(sew) cases.
            // Assuming SEW=8 for base functionality in this specific update
            
            for(int i=0; i<vl * sew; i++) 
            {
                if(i < vd.Length && i < vs1.Length && i < vs2.Length) 
                    vd[i] = (byte)(vs1[i] + vs2[i]);
            }
            s.VRegisters.WriteRaw(d.Rd, vd);
        }

        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op)
        {
            op.AluResult = 0; // Vector Execution typically side-effect based in this model
        }
    }

    // OPIVX (Vector-Scalar Arithmetic)
    [RiscvInstruction("VADD.VX", InstructionSet.RV64I, RiscvEncodingType.R, 0x57, Funct3 = 4, Funct6 = 0x00)]
    public class VaddVxInstruction : RTypeInstruction
    {
        public VaddVxInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

        public override void Execute(MachineState s, InstructionData d)
        {
            int vl = s.VRegisters.Vl;
            ulong scalar = s.Registers.Read(d.Rs1); // Scalar value
            var vs2 = s.VRegisters.GetRaw(d.Rs2);   // Vector register
            var vd = new byte[s.VRegisters.VLenBytes];

            byte scalarByte = (byte)(scalar & 0xFF); // Simplified to 8-bit scalar add

            for(int i=0; i<vl; i++) 
            {
                 if(i < vd.Length && i < vs2.Length)
                    vd[i] = (byte)(vs2[i] + scalarByte);
            }
            s.VRegisters.WriteRaw(d.Rd, vd);
        }
        
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) 
        {
            op.AluResult = 0;
        }
    }
}
