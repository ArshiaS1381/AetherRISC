using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A
{
    // LR.W
    [RiscvInstruction("LR.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x02)]
    public class LrWInstruction : RTypeInstruction {
        public LrWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, 0) { } // rs2 is 0/aq/rl
        public override void Execute(MachineState s, InstructionData d) {
            uint addr = (uint)s.Registers.Read(d.Rs1);
            s.LoadReservationAddress = addr;
            s.Registers.Write(d.Rd, (ulong)(long)(int)s.Memory!.ReadWord(addr));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = r1;
            s.LoadReservationAddress = r1;
        }
        public override bool IsLoad => true;
    }

    // SC.W
    [RiscvInstruction("SC.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x03)]
    public class ScWInstruction : RTypeInstruction {
        public ScWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            uint addr = (uint)s.Registers.Read(d.Rs1);
            if (s.LoadReservationAddress == addr) {
                s.Memory!.WriteWord(addr, (uint)s.Registers.Read(d.Rs2));
                s.Registers.Write(d.Rd, 0); // Success
            } else {
                s.Registers.Write(d.Rd, 1); // Failure
            }
            s.LoadReservationAddress = null;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = r1; // Address
            op.StoreValue = r2;
            
            // Check reservation
            if (s.LoadReservationAddress == r1) {
                op.FinalResult = 0; 
                // We flag as MemWrite
            } else {
                // Fail: 
                op.MemWrite = false; // Disable the memory write!
                op.FinalResult = 1;  // Error code to Rd
            }
            s.LoadReservationAddress = null;
        }
        public override bool IsStore => true;
    }
    
    // LR.D
    [RiscvInstruction("LR.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x02)]
    public class LrDInstruction : RTypeInstruction {
        public LrDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, 0) { }
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            s.LoadReservationAddress = addr;
            s.Registers.Write(d.Rd, s.Memory!.ReadDouble((uint)addr));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = r1;
            s.LoadReservationAddress = r1;
        }
        public override bool IsLoad => true;
    }

    // SC.D
    [RiscvInstruction("SC.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x03)]
    public class ScDInstruction : RTypeInstruction {
        public ScDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = s.Registers.Read(d.Rs1);
            if (s.LoadReservationAddress == addr) {
                s.Memory!.WriteDouble((uint)addr, s.Registers.Read(d.Rs2));
                s.Registers.Write(d.Rd, 0); 
            } else {
                s.Registers.Write(d.Rd, 1); 
            }
            s.LoadReservationAddress = null;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = r1;
            op.StoreValue = r2;
            if (s.LoadReservationAddress == r1) {
                op.FinalResult = 0; 
            } else {
                op.MemWrite = false;
                op.FinalResult = 1;
            }
            s.LoadReservationAddress = null;
        }
        public override bool IsStore => true;
    }

    // AMOSWAP.W
    [RiscvInstruction("AMOSWAP.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x01)]
    public class AmoSwapWInstruction : RTypeInstruction {
        public AmoSwapWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            uint addr = (uint)s.Registers.Read(d.Rs1);
            uint val = s.Memory!.ReadWord(addr);
            s.Memory.WriteWord(addr, (uint)s.Registers.Read(d.Rs2));
            s.Registers.Write(d.Rd, (ulong)(long)(int)val);
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            uint addr = (uint)r1;
            uint val = s.Memory!.ReadWord(addr); // Read
            op.StoreValue = r2; // Write value
            op.AluResult = r1; // Address
            op.FinalResult = (ulong)(long)(int)val; // Result to Rd
        }
        public override bool IsLoad => true;
        public override bool IsStore => true;
    }
    
    // AMOSWAP.D
    [RiscvInstruction("AMOSWAP.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x01)]
    public class AmoSwapDInstruction : RTypeInstruction {
        public AmoSwapDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            uint addr = (uint)s.Registers.Read(d.Rs1);
            ulong val = s.Memory!.ReadDouble(addr);
            s.Memory.WriteDouble(addr, s.Registers.Read(d.Rs2));
            s.Registers.Write(d.Rd, val);
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            uint addr = (uint)r1;
            ulong val = s.Memory!.ReadDouble(addr);
            op.StoreValue = r2;
            op.AluResult = r1;
            op.FinalResult = val;
        }
        public override bool IsLoad => true;
        public override bool IsStore => true;
    }
}
