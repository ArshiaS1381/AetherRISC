using System;
using System.Reflection;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding; // Correct namespace for Decompressor
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Utils;

namespace AetherRISC.Core.Architecture.Simulation.Runners;

public class SimpleRunner
{
    private readonly MachineState _state;
    private readonly ISimulationLogger _logger;
    private readonly InstructionDecoder _decoder;

    public SimpleRunner(MachineState state, ISimulationLogger logger)
    {
        _state = state;
        _logger = logger;
        _decoder = new InstructionDecoder();
    }

    public void Run(int maxCycles)
    {
        _logger.Initialize("CLI_Simulation");

        for (int cycle = 0; cycle < maxCycles; cycle++)
        {
            if (_state.Halted) break;

            if (_logger.IsVerbose) _logger.BeginCycle(cycle);

            ulong pc = _state.ProgramCounter;
            uint raw32;
            int instLen;

            try
            {
                // Fetch Logic
                ushort inst16 = _state.Memory!.ReadHalf((uint)pc);
                
                // Use the Decompressor from ISA.Decoding namespace
                if (InstructionDecompressor.IsCompressed(inst16)) {
                    raw32 = InstructionDecompressor.Decompress(inst16);
                    instLen = 2;
                } else {
                    raw32 = _state.Memory.ReadWord((uint)pc);
                    instLen = 4;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("ERR", $"Fetch Fault at 0x{pc:X}: {ex.Message}");
                break;
            }

            if (_logger.IsVerbose) _logger.LogStageFetch(pc, raw32);

            IInstruction? instruction = _decoder.Decode(raw32);

            if (instruction == null)
            {
                _logger.Log("ERR", $"Illegal Instruction at 0x{pc:X}: {raw32:X8}");
                _state.Halted = true; 
                break;
            }

            if (_logger.IsVerbose) _logger.LogStageDecode(pc, raw32, instruction);

            try
            {
                var data = BuildInstructionData(raw32, pc, instruction, _state);
                instruction.Execute(_state, data);

                if (_logger.IsVerbose)
                {
                    _logger.LogStageExecute(pc, raw32, instruction.Mnemonic);
                    if (data.Rd != 0 && !instruction.IsStore && !instruction.IsBranch)
                    {
                        ulong result = _state.Registers.Read(data.Rd);
                        _logger.LogStageWriteback(pc, raw32, data.Rd, result);
                    }
                }

                if (_state.ProgramCounter == pc)
                {
                    _state.ProgramCounter = pc + (ulong)instLen;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("ERR", $"Execution Fault: {ex.Message}");
                break;
            }

            // Ensure zero register is clean (double safety)
            _state.Registers.Write(0, 0); 
            
            if (_logger.IsVerbose) _logger.CompleteCycle();
        }

        _logger.FinalizeSession();
    }

    private static InstructionData BuildInstructionData(uint raw, ulong pc, IInstruction inst, MachineState state)
    {
        // Extract fields needed for Legacy Execute
        int rd = (int)((raw >> 7) & 0x1F);
        int rs1 = (int)((raw >> 15) & 0x1F);
        int rs2 = (int)((raw >> 20) & 0x1F);
        
        var attr = inst.GetType().GetCustomAttribute<RiscvInstructionAttribute>();
        var enc = attr?.Type ?? RiscvEncodingType.Custom;

        int imm = 0;
        ulong immediate = 0;

        // Fast decode immediate based on type
        switch (enc)
        {
            case RiscvEncodingType.I: imm = BitUtils.ExtractITypeImm(raw); break;
            case RiscvEncodingType.S: imm = BitUtils.ExtractSTypeImm(raw); break;
            case RiscvEncodingType.B: imm = BitUtils.ExtractBTypeImm(raw); break;
            case RiscvEncodingType.U: imm = BitUtils.ExtractUTypeImm(raw); immediate = (ulong)(uint)imm; break;
            case RiscvEncodingType.J: imm = BitUtils.ExtractJTypeImm(raw); break;
            case RiscvEncodingType.ShiftImm: imm = BitUtils.ExtractShamt(raw, state.Config.XLEN); break;
            default: imm = 0; break;
        }
        
        if (enc != RiscvEncodingType.U && enc != RiscvEncodingType.ShiftImm)
            immediate = (ulong)(long)imm; // Sign extend by default for arithmetic

        // Use properties from instance if custom/unknown
        if (enc == RiscvEncodingType.Custom) {
            rd = inst.Rd; rs1 = inst.Rs1; rs2 = inst.Rs2; imm = inst.Imm;
            immediate = (ulong)(long)imm;
        }

        return new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Imm = imm, Immediate = immediate, PC = pc };
    }
}
