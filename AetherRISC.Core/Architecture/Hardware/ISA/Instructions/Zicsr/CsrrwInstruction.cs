using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;

[RiscvInstruction("CSRRW", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 1, 
    Name = "CSR Read/Write", 
    Description = "Atomically swaps the value in a Control and Status Register (CSR) with a general-purpose register.", 
    Usage = "csrrw rd, csr, rs1")]
public class CsrrwInstruction : ITypeInstruction
{
    public CsrrwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        // CSR address is 12-bit unsigned, mask off sign extension
        uint csrAddr = (uint)d.Imm & 0xFFFu;
        ulong oldVal = s.Csr.Read(csrAddr);
        s.Csr.Write(csrAddr, s.Registers.Read(d.Rs1));
        s.Registers.Write(d.Rd, oldVal);
    }
}
