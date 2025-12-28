using System;
using System.Reflection;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Simulation.State;

using AetherRISC.Core.Assembler;

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
            if (_state.Halted)
            {
                _logger.Log("SYS", "CPU Halted.");
                break;
            }

            _logger.BeginCycle(cycle);

            if (_state.Memory == null)
            {
                _logger.Log("ERR", "System Memory is not initialized.");
                break;
            }

            ulong pc = _state.ProgramCounter;

            uint raw32;
            int instLen;

            try
            {
                ushort inst16 = _state.Memory.ReadHalf((uint)pc);

                if (InstructionDecompressor.IsCompressed(inst16))
                {
                    raw32 = InstructionDecompressor.Decompress(inst16);
                    instLen = 2;
                }
                else
                {
                    raw32 = _state.Memory.ReadWord((uint)pc);
                    instLen = 4;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("ERR", $"Fetch Fault at 0x{pc:X}: {ex.Message}");
                break;
            }

            _logger.LogStageFetch(pc, raw32);

            IInstruction? instruction;
            try
            {
                instruction = _decoder.Decode(raw32);
            }
            catch (Exception ex)
            {
                _logger.Log("ERR", $"Decode Fault: {ex.Message}");
                break;
            }

            if (instruction == null)
            {
                _logger.Log("ERR", $"Illegal Instruction at 0x{pc:X}: {raw32:X8}");
                break;
            }

            _logger.LogStageDecode(pc, raw32, instruction);

            try
            {
                var data = BuildInstructionData(raw32, pc, instruction, _state);

                instruction.Execute(_state, data);

                _logger.LogStageExecute(pc, raw32, instruction.Mnemonic);

                if (data.Rd != 0 && !instruction.IsStore && !instruction.IsBranch)
                {
                    ulong result = _state.Registers.Read(data.Rd);
                    _logger.LogStageWriteback(pc, raw32, data.Rd, result);
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

            _state.Registers.Write(0, 0);
            _logger.CompleteCycle();
        }

        _logger.FinalizeSession();
    }

    private static InstructionData BuildInstructionData(
        uint raw,
        ulong pc,
        IInstruction inst,
        MachineState state
    )
    {
        int rd = (int)((raw >> 7) & 0x1F);
        int rs1 = (int)((raw >> 15) & 0x1F);
        int rs2 = (int)((raw >> 20) & 0x1F);
        
        var attr = inst.GetType().GetCustomAttribute<RiscvInstructionAttribute>();
        var enc = attr?.Type ?? RiscvEncodingType.Custom;

        int imm = 0;
        ulong immediate = 0;

        switch (enc)
        {
            case RiscvEncodingType.R:
                imm = 0; immediate = 0;
                break;

            case RiscvEncodingType.I:
                imm = BitUtils.ExtractITypeImm(raw);
                immediate = (ulong)(long)imm;
                break;

            case RiscvEncodingType.S:
                imm = BitUtils.ExtractSTypeImm(raw);
                immediate = (ulong)(long)imm;
                break;

            case RiscvEncodingType.B:
                imm = BitUtils.ExtractBTypeImm(raw);
                immediate = (ulong)(long)imm;
                break;

            case RiscvEncodingType.U:
                imm = BitUtils.ExtractUTypeImm(raw);
                immediate = (ulong)(uint)imm;
                break;

            case RiscvEncodingType.J:
                imm = BitUtils.ExtractJTypeImm(raw);
                immediate = (ulong)(long)imm;
                break;

            case RiscvEncodingType.ShiftImm:
                imm = BitUtils.ExtractShamt(raw, state.Config.XLEN);
                immediate = (ulong)imm;
                break;

            case RiscvEncodingType.ZbbUnary:
                imm = 0; immediate = 0;
                break;

            default:
                rd = inst.Rd; rs1 = inst.Rs1; rs2 = inst.Rs2; imm = inst.Imm;
                immediate = (ulong)(long)imm;
                break;
        }

        return new InstructionData
        {
            Rd = rd, Rs1 = rs1, Rs2 = rs2, Imm = imm, Immediate = immediate, PC = pc
        };
    }
}
