using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("REMW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 6, Funct7 = 1,
    Name = "Remainder Word", Description = "Signed remainder of 32-bit division, sign-extended to 64 bits.", Usage = "remw rd, rs1, rs2")]
public class RemwInstruction : RTypeInstruction
{
    public RemwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) => 
        s.Registers.Write(d.Rd, Calc((int)s.Registers.Read(d.Rs1), (int)s.Registers.Read(d.Rs2)));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc((int)rs1Val, (int)rs2Val);
    }

    private ulong Calc(int v1, int v2)
    {
        if (v2 == 0) return (ulong)(long)v1;
        if (v1 == int.MinValue && v2 == -1) return 0;
        return (ulong)(long)(v1 % v2);
    }
}

[RiscvInstruction("REMUW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 7, Funct7 = 1,
    Name = "Remainder Word Unsigned", Description = "Unsigned remainder of 32-bit division, sign-extended to 64 bits.", Usage = "remuw rd, rs1, rs2")]
public class RemuwInstruction : RTypeInstruction
{
    public RemuwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d)
    {
        uint v1 = (uint)s.Registers.Read(d.Rs1);
        uint v2 = (uint)s.Registers.Read(d.Rs2);
        s.Registers.Write(d.Rd, v2 == 0 ? (ulong)(long)(int)v1 : (ulong)(long)(int)(v1 % v2));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint v1 = (uint)rs1Val;
        uint v2 = (uint)rs2Val;
        buffers.ExecuteMemory.AluResult = v2 == 0 ? (ulong)(long)(int)v1 : (ulong)(long)(int)(v1 % v2);
    }
}
