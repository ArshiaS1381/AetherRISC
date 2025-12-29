using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class FetchStage
    {
        private readonly MachineState _state;

        public FetchStage(MachineState state)
        {
            _state = state;
        }

        public void Run(PipelineBuffers buffers, IBranchPredictor predictor)
        {
            if (buffers.FetchDecode.IsStalled) return;

            ulong currentPC = _state.Registers.PC;
            
            // 1. Physical Fetch
            ushort lower = _state.Memory!.ReadHalf((uint)currentPC);
            uint finalInstruction;
            int step;

            if (InstructionDecompressor.IsCompressed(lower))
            {
                finalInstruction = InstructionDecompressor.Decompress(lower);
                step = 2;
            }
            else
            {
                finalInstruction = _state.Memory.ReadWord((uint)currentPC);
                step = 4;
            }

            // 2. Branch Prediction
            var prediction = predictor.Predict(currentPC);
            
            // 3. Push to Pipeline Buffer
            buffers.FetchDecode.Instruction = finalInstruction;
            buffers.FetchDecode.PC = currentPC;
            
            // Pass prediction info down the pipe
            buffers.FetchDecode.PredictedTaken = prediction.PredictedTaken;
            buffers.FetchDecode.PredictedTarget = prediction.TargetAddress;

            buffers.FetchDecode.IsValid = true;
            buffers.FetchDecode.IsEmpty = false;

            // 4. Update PC (Speculative)
            if (prediction.PredictedTaken && prediction.TargetAddress != 0)
            {
                // Speculatively jump!
                _state.Registers.PC = prediction.TargetAddress;
            }
            else
            {
                // Proceed sequentially
                _state.Registers.PC += (ulong)step;
            }
        }
    }
}
