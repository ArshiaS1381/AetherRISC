using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
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
}
