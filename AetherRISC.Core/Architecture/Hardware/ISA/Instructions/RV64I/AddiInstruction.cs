using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("ADDI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 0,
    Name = "Add Immediate",
    Description = "Adds the sign-extended 12-bit immediate to register rs1. In RV64, the result is 64-bit.",
    Usage = "addi rd, rs1, imm")]
public class AddiInstruction : ITypeInstruction
{
    public AddiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong res = s.Registers.Read(d.Rs1) + d.Immediate;
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // Fix: Explicitly cast immediate to long then ulong to ensure correct operator overload is chosen
        ulong res = rs1Val + (ulong)(long)buffers.DecodeExecute.Immediate;
        if (state.Config.XLEN == 32) res = (ulong)(uint)res;
        buffers.ExecuteMemory.AluResult = res;
    }
}
