using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;

[RiscvInstruction("CSRRSI", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 6, 
    Name = "CSR Read and Set Immediate", 
    Description = "Sets bits in the CSR based on a zero-extended 5-bit immediate bitmask.", 
    Usage = "csrrsi rd, csr, uimm")]
public class CsrrsiInstruction : ITypeInstruction
{
    public CsrrsiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint csrAddr = (uint)d.Immediate;
        ulong oldVal = s.Csr.Read(csrAddr);
        ulong uimm = (ulong)(uint)d.Rs1;
        if (uimm != 0) s.Csr.Write(csrAddr, oldVal | uimm);
        s.Registers.Write(d.Rd, oldVal);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint csrAddr = (uint)buffers.DecodeExecute.Immediate & 0xFFFu;
        ulong oldVal = state.Csr.Read(csrAddr);
        ulong uimm = (ulong)(this.Rs1 & 0x1F);

        if (uimm != 0)
        {
            state.Csr.Write(csrAddr, oldVal | uimm);
        }
        
        buffers.ExecuteMemory.AluResult = oldVal;
    }
}
