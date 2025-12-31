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

            buffers.FetchDecode.Flush();
            buffers.FetchDecode.SetHasContent();

            ulong currentPC = _state.ProgramCounter;
            int fetchCap = buffers.FetchDecode.Slots.Length;
            int filled = 0;

            // Block size determination
            int blockSize = _settings.EnableBlockFetching ? _settings.FetchBlockSize : 4;
            int bytesFetched = 0;

            while (filled < fetchCap)
            {
                // Enforce block boundary if block fetching is enabled
                if (_settings.EnableBlockFetching && bytesFetched >= blockSize) break;

                // Stop at memory boundary
                if (currentPC > 0xFFFFFFF0) break;

                try
                {
                    if (_state.Memory == null) break;
                    
                    ushort lower16 = _state.Memory.ReadHalf((uint)currentPC);
                    bool isCompressed = InstructionDecompressor.IsCompressed(lower16);
                    uint finalRaw;
                    ulong instSize;

                    if (isCompressed)
                    {
                        finalRaw = InstructionDecompressor.Decompress(lower16);
                        instSize = 2;
                    }
                    else
                    {
                        ushort upper16 = _state.Memory.ReadHalf((uint)(currentPC + 2));
                        uint fullRaw = (uint)lower16 | ((uint)upper16 << 16);
                        finalRaw = fullRaw;
                        instSize = 4;
                    }

                    var slot = buffers.FetchDecode.Slots[filled];
                    slot.Valid = true;
                    slot.IsBubble = false;
                    slot.PC = currentPC;
                    slot.RawInstruction = finalRaw;

                    // Prediction
                    var prediction = PredictBranch(finalRaw, currentPC, instSize, predictor);
                    slot.PredictedTaken = prediction.Taken;
                    slot.PredictedTarget = prediction.Target;

                    filled++;
                    bytesFetched += (int)instSize;

                    if (prediction.Taken)
                    {
                        // Optimization: Dynamic Branch Fetching
                        if (_settings.EnableDynamicBranchFetching)
                        {
                            currentPC = prediction.Target;
                            // Breaking here simulates the pipeline bubble/redirect latency inherent in taken branches
                            // even with dynamic fetching, we usually start a new block at the new address in the NEXT cycle
                            // or immediate if hardware is complex. We break here to simplify (new PC is set).
                            break; 
                        }
                        else
                        {
                            // Static Fetch behavior: Continue sequential fetch, Execute stage will detect mispredict/redirect later
                            currentPC += instSize;
                        }
                    }
                    else
                    {
                        currentPC += instSize;
                    }
                }
                catch
                {
                    break;
                }
            }

            _state.ProgramCounter = currentPC;
        }

        private (bool Taken, ulong Target) PredictBranch(uint raw, ulong pc, ulong size, IBranchPredictor predictor)
        {
            uint opcode = raw & 0x7F;
            
            // JAL
            if (opcode == 0x6F) 
            {
                int imm = (int)Hardware.ISA.Utils.BitUtils.ExtractJTypeImm(raw);
                return (true, pc + (ulong)imm);
            }

            // Branches
            if (opcode == 0x63)
            {
                var pred = predictor.Predict(pc);
                return (pred.PredictedTaken, pred.TargetAddress);
            }

            // JALR (Ret/Call)
            if (opcode == 0x67)
            {
                uint rd = (raw >> 7) & 0x1F;
                uint rs1 = (raw >> 15) & 0x1F;
                if (rd == 0 && (rs1 == 1 || rs1 == 5) && _settings.EnableReturnAddressStack)
                    return (true, _ras.Pop());
                if ((rd == 1 || rd == 5) && _settings.EnableReturnAddressStack)
                    _ras.Push(pc + size);
            }
            
            return (false, 0);
        }
    }
}
