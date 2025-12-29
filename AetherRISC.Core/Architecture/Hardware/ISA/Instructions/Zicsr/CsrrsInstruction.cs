using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;

[RiscvInstruction("CSRRS", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 2, 
    Name = "CSR Read and Set Bits", 
    Description = "Reads the CSR value into rd and sets bits in the CSR based on the bitmask in rs1.", 
    Usage = "csrrs rd, csr, rs1")]
public class CsrrsInstruction : ITypeInstruction
{
    public CsrrsInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint csrAddr = (uint)d.Imm & 0xFFFu;
        ulong oldVal = s.Csr.Read(csrAddr);
        if (d.Rs1 != 0) s.Csr.Write(csrAddr, oldVal | s.Registers.Read(d.Rs1));
        s.Registers.Write(d.Rd, oldVal);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint csrAddr = (uint)buffers.DecodeExecute.Immediate & 0xFFFu;
        ulong oldVal = state.Csr.Read(csrAddr);
        // Only write if rs1 is not x0, OR if we strictly follow spec "if rs1 != 0"
        // In the pipeline, rs1Val is the *value*. We need to check if the *register index* was 0.
        // However, standard says if rs1 register index is x0, no write occurs.
        // buffers.DecodeExecute.DecodedInst.Rs1 holds the index.
        
        if (this.Rs1 != 0) 
        {
            state.Csr.Write(csrAddr, oldVal | rs1Val);
        }
        
        buffers.ExecuteMemory.AluResult = oldVal;
    }
}
