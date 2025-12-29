using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("AMOMINU.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x18)]
public class AmoMinuWInstruction : RTypeInstruction
{
    public AmoMinuWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        uint val = (uint)s.Registers.Read(d.Rs2);
        uint mem = s.Memory!.ReadWord(addr);
        s.Registers.Write(d.Rd, (ulong)(long)(int)mem);
        s.Memory.WriteWord(addr, Math.Min(mem, val));
    }
    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers) { buffers.ExecuteMemory.AluResult = rs1Val; buffers.ExecuteMemory.StoreValue = rs2Val; }
}

[RiscvInstruction("AMOMAXU.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x1C)]
public class AmoMaxuWInstruction : RTypeInstruction
{
    public AmoMaxuWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        uint val = (uint)s.Registers.Read(d.Rs2);
        uint mem = s.Memory!.ReadWord(addr);
        s.Registers.Write(d.Rd, (ulong)(long)(int)mem);
        s.Memory.WriteWord(addr, Math.Max(mem, val));
    }
    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers) { buffers.ExecuteMemory.AluResult = rs1Val; buffers.ExecuteMemory.StoreValue = rs2Val; }
}

[RiscvInstruction("AMOMINU.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x18)]
public class AmoMinuDInstruction : RTypeInstruction
{
    public AmoMinuDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        ulong val = s.Registers.Read(d.Rs2);
        ulong mem = s.Memory!.ReadDouble(addr);
        s.Registers.Write(d.Rd, mem);
        s.Memory.WriteDouble(addr, Math.Min(mem, val));
    }
    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers) { buffers.ExecuteMemory.AluResult = rs1Val; buffers.ExecuteMemory.StoreValue = rs2Val; }
}

[RiscvInstruction("AMOMAXU.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x1C)]
public class AmoMaxuDInstruction : RTypeInstruction
{
    public AmoMaxuDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        ulong val = s.Registers.Read(d.Rs2);
        ulong mem = s.Memory!.ReadDouble(addr);
        s.Registers.Write(d.Rd, mem);
        s.Memory.WriteDouble(addr, Math.Max(mem, val));
    }
    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers) { buffers.ExecuteMemory.AluResult = rs1Val; buffers.ExecuteMemory.StoreValue = rs2Val; }
}
