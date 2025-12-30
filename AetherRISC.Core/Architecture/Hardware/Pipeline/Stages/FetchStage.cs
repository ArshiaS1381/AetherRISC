using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class FetchStage
    {
        private readonly MachineState _state;
        private readonly ArchitectureSettings _settings;
        private readonly ReturnAddressStack _ras;

        public FetchStage(MachineState state, ArchitectureSettings settings)
        {
            _state = state;
            _settings = settings ?? new ArchitectureSettings();
            _ras = new ReturnAddressStack(16);
        }

        public void Run(PipelineBuffers buffers, IBranchPredictor predictor)
        {
            if (buffers.FetchDecode.IsStalled) return;

            // Note: We flush the buffer. If Decode didn't consume everything, 
            // DecodeStage logic must have reset the PC to the first unconsumed instruction.
            buffers.FetchDecode.Flush();
            buffers.FetchDecode.SetHasContent();

            ulong currentPC = _state.ProgramCounter;
            var sysBus = _state.Memory as SystemBus;
            
            // Fetch up to the Physical Buffer Capacity (determined by Width * FetchRatio)
            int fetchCap = buffers.FetchDecode.Slots.Length;

            for (int i = 0; i < fetchCap; i++)
            {
                var slot = buffers.FetchDecode.Slots[i];
                
                try 
                {
                    uint rawWord;
                    if (sysBus != null) rawWord = sysBus.ReadWord((uint)currentPC);
                    else rawWord = _state.Memory!.ReadWord((uint)currentPC);
                    
                    if (rawWord == 0 || rawWord == 0xFFFFFFFF) 
                    {
                        slot.Reset();
                        break; 
                    }

                    ushort lower16 = (ushort)(rawWord & 0xFFFF);
                    bool isCompressed = InstructionDecompressor.IsCompressed(lower16);
                    uint finalRaw = isCompressed ? InstructionDecompressor.Decompress(lower16) : rawWord;
                    ulong instSize = isCompressed ? 2u : 4u;

                    slot.Valid = true;
                    slot.IsBubble = false;
                    slot.PC = currentPC;
                    slot.RawInstruction = finalRaw;

                    // Prediction Logic
                    bool taken = false;
                    ulong target = 0;
                    uint opcode = finalRaw & 0x7F;
                    uint rd = (finalRaw >> 7) & 0x1F;
                    uint rs1 = (finalRaw >> 15) & 0x1F;
                    
                    bool isLink = (rd == 1 || rd == 5); 
                    // Fix: opcode 0x67 is JALR.
                    bool isJal = opcode == 0x6F;
                    bool isJalr = opcode == 0x67;
                    bool isRet = isJalr && rd == 0 && (rs1 == 1 || rs1 == 5); 

                    if (_settings.EnableReturnAddressStack && isRet)
                    {
                        taken = true; target = _ras.Pop();
                    }
                    else if (_settings.EnableReturnAddressStack && (isJal || isJalr) && isLink)
                    {
                        _ras.Push(currentPC + instSize);
                        var pred = predictor.Predict(currentPC);
                        taken = pred.PredictedTaken; target = pred.TargetAddress;
                    }
                    else
                    {
                        var pred = predictor.Predict(currentPC);
                        taken = pred.PredictedTaken; target = pred.TargetAddress;
                    }

                    slot.PredictedTaken = taken;
                    slot.PredictedTarget = target;

                    if (taken)
                    {
                        currentPC = target;
                        // If we branch, we effectively break the sequential fetch block.
                        // If AllowDynamicBranchFetching is ON, we effectively stop filling the buffer here
                        // because we jumped to a new location.
                        // If it is OFF, we definitely stop because we can't fetch non-contiguously in one cycle easily
                        // without multi-ported I-Cache simulation.
                        break;
                    }
                    else
                    {
                        currentPC += instSize;
                    }
                }
                catch
                {
                    slot.Reset();
                    break;
                }
            }

            _state.ProgramCounter = currentPC;
        }
    }
}
