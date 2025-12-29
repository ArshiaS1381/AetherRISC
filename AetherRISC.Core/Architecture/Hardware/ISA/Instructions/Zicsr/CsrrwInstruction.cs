using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;

[RiscvInstruction("CSRRW", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 1, 
    Name = "CSR Read/Write", 
    Description = "Atomically swaps the value in a CSR with a general-purpose register.", 
    Usage = "csrrw rd, csr, rs1")]
public class CsrrwInstruction : ITypeInstruction
{
    public CsrrwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint csrAddr = (uint)d.Imm & 0xFFFu;
        ulong oldVal = s.Csr.Read(csrAddr);
        s.Csr.Write(csrAddr, s.Registers.Read(d.Rs1));
        s.Registers.Write(d.Rd, oldVal);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint csrAddr = (uint)buffers.DecodeExecute.Immediate & 0xFFFu;
        ulong oldVal = state.Csr.Read(csrAddr);
        state.Csr.Write(csrAddr, rs1Val);
        buffers.ExecuteMemory.AluResult = oldVal;
    }
}
