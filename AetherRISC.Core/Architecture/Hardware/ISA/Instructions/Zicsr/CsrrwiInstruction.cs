using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;

[RiscvInstruction("CSRRWI", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 5, 
    Name = "CSR Read/Write Immediate", 
    Description = "Swaps the CSR value with a zero-extended 5-bit immediate. The old value is stored in rd.", 
    Usage = "csrrwi rd, csr, uimm")]
public class CsrrwiInstruction : ITypeInstruction
{
    public CsrrwiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint csrAddr = (uint)d.Imm & 0xFFFu;
        ulong oldVal = s.Csr.Read(csrAddr);
        // rs1 field contains the 5-bit immediate for CSRRWI
        s.Csr.Write(csrAddr, (ulong)(d.Rs1 & 0x1F));
        s.Registers.Write(d.Rd, oldVal);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint csrAddr = (uint)buffers.DecodeExecute.Immediate & 0xFFFu;
        ulong oldVal = state.Csr.Read(csrAddr);
        // For Imm instructions, the 5-bit uimm is in the Rs1 field index
        ulong uimm = (ulong)(this.Rs1 & 0x1F);
        
        state.Csr.Write(csrAddr, uimm);
        buffers.ExecuteMemory.AluResult = oldVal;
    }
}
