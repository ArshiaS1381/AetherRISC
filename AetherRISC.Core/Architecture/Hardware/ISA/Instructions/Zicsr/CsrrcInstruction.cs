using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;

[RiscvInstruction("CSRRC", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 3, 
    Name = "CSR Read and Clear Bits", 
    Description = "Reads the CSR value into rd and clears bits in the CSR based on the bitmask in rs1.", 
    Usage = "csrrc rd, csr, rs1")]
public class CsrrcInstruction : ITypeInstruction
{
    public CsrrcInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint csrAddr = (uint)d.Immediate;
        ulong oldVal = s.Csr.Read(csrAddr);
        if (d.Rs1 != 0) s.Csr.Write(csrAddr, oldVal & ~s.Registers.Read(d.Rs1));
        s.Registers.Write(d.Rd, oldVal);
    }
}
