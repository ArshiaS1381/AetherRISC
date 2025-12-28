using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("AMOMIN.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x10)]
public class AmoMinWInstruction : RTypeInstruction
{
    public AmoMinWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        int val = (int)s.Registers.Read(d.Rs2);
        int mem = (int)s.Memory!.ReadWord(addr);
        s.Registers.Write(d.Rd, (ulong)(long)mem);
        s.Memory.WriteWord(addr, (uint)Math.Min(mem, val));
    }
}

[RiscvInstruction("AMOMAX.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x14)]
public class AmoMaxWInstruction : RTypeInstruction
{
    public AmoMaxWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        int val = (int)s.Registers.Read(d.Rs2);
        int mem = (int)s.Memory!.ReadWord(addr);
        s.Registers.Write(d.Rd, (ulong)(long)mem);
        s.Memory.WriteWord(addr, (uint)Math.Max(mem, val));
    }
}

[RiscvInstruction("AMOMIN.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x10)]
public class AmoMinDInstruction : RTypeInstruction
{
    public AmoMinDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        long val = (long)s.Registers.Read(d.Rs2);
        long mem = (long)s.Memory!.ReadDouble(addr);
        s.Registers.Write(d.Rd, (ulong)mem);
        s.Memory.WriteDouble(addr, (ulong)Math.Min(mem, val));
    }
}

[RiscvInstruction("AMOMAX.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x14)]
public class AmoMaxDInstruction : RTypeInstruction
{
    public AmoMaxDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        long val = (long)s.Registers.Read(d.Rs2);
        long mem = (long)s.Memory!.ReadDouble(addr);
        s.Registers.Write(d.Rd, (ulong)mem);
        s.Memory.WriteDouble(addr, (ulong)Math.Max(mem, val));
    }
}
