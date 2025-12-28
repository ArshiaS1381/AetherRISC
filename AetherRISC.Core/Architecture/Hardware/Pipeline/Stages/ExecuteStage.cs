using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class ExecuteStage
    {
        private readonly MachineState _state;

        public ExecuteStage(MachineState state)
        {
            _state = state;
        }

        public void Run(PipelineBuffers buffers)
        {
            if (buffers.DecodeExecute.IsEmpty) 
            {
                buffers.ExecuteMemory.Flush();
                return;
            }

            var input = buffers.DecodeExecute;
            var output = buffers.ExecuteMemory;

            ulong rs1Val = input.ForwardedRs1 ?? _state.Registers[input.DecodedInst?.Rs1 ?? 0];
            ulong rs2Val = input.ForwardedRs2 ?? _state.Registers[input.DecodedInst?.Rs2 ?? 0];

            output.DecodedInst = input.DecodedInst;
            output.RawInstruction = input.RawInstruction;
            output.PC = input.PC;
            output.Rd = input.Rd;
            output.RegWrite = input.RegWrite;
            output.MemRead = input.MemRead;
            output.MemWrite = input.MemWrite;
            output.StoreValue = rs2Val;
            output.BranchTaken = false;

            ulong result = 0;
            long imm = (long)input.Immediate;

            if (input.DecodedInst != null)
            {
                // Normalize name: "SlliInstruction" -> "SLLI"
                var className = input.DecodedInst.GetType().Name.ToUpper();
                var name = className.Replace("INSTRUCTION", "");

                // --- BASE INTEGER ---
                if (name == "ADD") result = rs1Val + rs2Val;
                else if (name == "SUB") result = rs1Val - rs2Val;
                else if (name == "ADDI" || name == "LB" || name == "LH" || name == "LW" || name == "LD" || name == "SB" || name == "SH" || name == "SW" || name == "SD") 
                    result = rs1Val + (ulong)imm;
                else if (name == "ADDIW") result = (ulong)(int)(rs1Val + (ulong)imm);
                
                // Logical Ops (R-Type)
                else if (name == "AND") result = rs1Val & rs2Val;
                else if (name == "OR")  result = rs1Val | rs2Val;
                else if (name == "XOR") result = rs1Val ^ rs2Val;
                
                // Logical Ops (I-Type) - Missing in previous version
                else if (name == "ANDI") result = rs1Val & (ulong)imm;
                else if (name == "ORI")  result = rs1Val | (ulong)imm;
                else if (name == "XORI") result = rs1Val ^ (ulong)imm;

                // Shifts (R-Type)
                else if (name == "SLL") result = rs1Val << (int)(rs2Val & 0x3F);
                else if (name == "SRL") result = rs1Val >> (int)(rs2Val & 0x3F);
                else if (name == "SRA") result = (ulong)((long)rs1Val >> (int)(rs2Val & 0x3F));
                else if (name == "SLLW") result = (ulong)((int)rs1Val << (int)(rs2Val & 0x1F));
                else if (name == "SRLW") result = (ulong)((uint)rs1Val >> (int)(rs2Val & 0x1F));
                else if (name == "SRAW") result = (ulong)((int)rs1Val >> (int)(rs2Val & 0x1F));

                // Shifts (I-Type) - CRITICAL FIX: SLLI was missing!
                else if (name == "SLLI") result = rs1Val << (int)(imm & 0x3F);
                else if (name == "SRLI") result = rs1Val >> (int)(imm & 0x3F);
                else if (name == "SRAI") result = (ulong)((long)rs1Val >> (int)(imm & 0x3F));
                else if (name == "SLLIW") result = (ulong)((int)rs1Val << (int)(imm & 0x1F));
                else if (name == "SRLIW") result = (ulong)((uint)rs1Val >> (int)(imm & 0x1F));
                else if (name == "SRAIW") result = (ulong)((int)rs1Val >> (int)(imm & 0x1F));

                // Upper Immediates
                else if (name == "LUI") result = (ulong)imm;
                else if (name == "AUIPC") result = input.PC + (ulong)imm;
                
                // Set Less Than
                else if (name == "SLT") result = ((long)rs1Val < (long)rs2Val) ? 1UL : 0UL;
                else if (name == "SLTI") result = ((long)rs1Val < imm) ? 1UL : 0UL;
                else if (name == "SLTU") result = (rs1Val < rs2Val) ? 1UL : 0UL;
                else if (name == "SLTIU") result = (rs1Val < (ulong)imm) ? 1UL : 0UL;
                
                // --- M EXTENSION ---
                else if (name == "MUL") result = rs1Val * rs2Val;
                else if (name == "MULH") { 
                   long a = (long)rs1Val;
                   long b = (long)rs2Val;
                   Int128 full = (Int128)a * b;
                   result = (ulong)(full >> 64);
                }
                else if (name == "DIV") {
                    long a = (long)rs1Val;
                    long b = (long)rs2Val;
                    if (b == 0) result = ulong.MaxValue;
                    else if (a == long.MinValue && b == -1) result = unchecked((ulong)long.MinValue);
                    else result = (ulong)(a / b);
                }
                else if (name == "DIVU") {
                    if (rs2Val == 0) result = ulong.MaxValue;
                    else result = rs1Val / rs2Val;
                }
                else if (name == "REM") {
                    long a = (long)rs1Val;
                    long b = (long)rs2Val;
                    if (b == 0) result = rs1Val;
                    else if (a == long.MinValue && b == -1) result = 0;
                    else result = (ulong)(a % b);
                }
                else if (name == "REMU") {
                    if (rs2Val == 0) result = rs1Val;
                    else result = rs1Val % rs2Val;
                }

                // --- Zbs ---
                else if (name == "BSET") result = rs1Val | (1UL << (int)(rs2Val & 0x3F));
                else if (name == "BCLR") result = rs1Val & ~(1UL << (int)(rs2Val & 0x3F));
                else if (name == "BINV") result = rs1Val ^ (1UL << (int)(rs2Val & 0x3F));
                else if (name == "BEXT") result = (rs1Val >> (int)(rs2Val & 0x3F)) & 1;

                // --- Zbc ---
                else if (name == "CLMUL") 
                {
                    result = 0;
                    ulong a = rs1Val;
                    ulong b = rs2Val;
                    for (int i = 0; i < 64; i++) {
                        if ((b & 1) != 0) result ^= a;
                        a <<= 1;
                        b >>= 1;
                    }
                }

                // --- CONTROL FLOW ---
                bool branchTaken = false;
                
                if (name == "BEQ") branchTaken = rs1Val == rs2Val;
                else if (name == "BNE") branchTaken = rs1Val != rs2Val;
                else if (name == "BLT") branchTaken = (long)rs1Val < (long)rs2Val;
                else if (name == "BGE") branchTaken = (long)rs1Val >= (long)rs2Val;
                else if (name == "BLTU") branchTaken = rs1Val < rs2Val;
                else if (name == "BGEU") branchTaken = rs1Val >= rs2Val;
                
                if (branchTaken)
                {
                     _state.Registers.PC = input.PC + (ulong)imm;
                     output.BranchTaken = true;
                }
                else if (name == "JAL")
                {
                    _state.Registers.PC = input.PC + (ulong)imm;
                    result = input.PC + 4; 
                    output.BranchTaken = true;
                }
                else if (name == "JALR")
                {
                    _state.Registers.PC = (rs1Val + (ulong)imm) & ~1UL;
                    result = input.PC + 4; 
                    output.BranchTaken = true;
                }
            }

            output.AluResult = result;
            output.IsEmpty = false;
        }
    }
}
