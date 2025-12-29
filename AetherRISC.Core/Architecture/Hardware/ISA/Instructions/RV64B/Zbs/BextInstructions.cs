using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbs;

[RiscvInstruction("BEXT", InstructionSet.Zbs, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x24,
    Name = "Bit Extract",
    Description = "Extracts a single bit from rs1 and stores it in the LSB of rd.",
    Usage = "bext rd, rs1, rs2")]
public class BextInstruction : RTypeInstruction
{
    public BextInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) >> (int)(s.Registers.Read(d.Rs2) & (s.Config.XLEN == 32 ? 31u : 63u))) & 1UL);

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int bit = (int)(rs2Val & (state.Config.XLEN == 32 ? 31u : 63u));
        buffers.ExecuteMemory.AluResult = (rs1Val >> bit) & 1UL;
    }
}

[RiscvInstruction("BEXTI", InstructionSet.Zbs, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x12,
    Name = "Bit Extract Immediate",
    Description = "Extracts a single bit from rs1 using the immediate index.",
    Usage = "bexti rd, rs1, shamt")]
public class BextiInstruction : ITypeInstruction
{
    public BextiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
    
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) >> (d.Imm & (s.Config.XLEN == 32 ? 31 : 63))) & 1UL);

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int imm = buffers.DecodeExecute.Immediate;
        int bit = imm & (state.Config.XLEN == 32 ? 31 : 63);
        buffers.ExecuteMemory.AluResult = (rs1Val >> bit) & 1UL;
    }
}
