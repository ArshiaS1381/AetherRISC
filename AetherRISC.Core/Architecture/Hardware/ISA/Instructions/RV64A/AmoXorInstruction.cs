using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("AMOXOR.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x04)]
public class AmoXorWInstruction : RTypeInstruction
{
    public AmoXorWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        uint val = (uint)s.Registers.Read(d.Rs2);
        uint mem = s.Memory!.ReadWord(addr);
        s.Registers.Write(d.Rd, (ulong)(long)(int)mem);
        s.Memory.WriteWord(addr, mem ^ val);
    }
}

[RiscvInstruction("AMOXOR.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x04)]
public class AmoXorDInstruction : RTypeInstruction
{
    public AmoXorDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        ulong val = s.Registers.Read(d.Rs2);
        ulong mem = s.Memory!.ReadDouble(addr);
        s.Registers.Write(d.Rd, mem);
        s.Memory.WriteDouble(addr, mem ^ val);
    }
}
