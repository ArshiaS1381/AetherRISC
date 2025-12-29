using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Stages;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Controller
{
    public class PipelineController
    {
        private readonly PipelineBuffers _buffers;
        public PipelineBuffers Buffers => _buffers;
        public IBranchPredictor Predictor { get; }

        private readonly FetchStage _fetch;
        private readonly DecodeStage _decode;
        private readonly ExecuteStage _execute;
        private readonly MemoryStage _memory;
        private readonly WritebackStage _writeback;

        private readonly DataHazardUnit _dataHazard;
        private readonly StructuralHazardUnit _structHazard;
        private readonly ControlHazardUnit _controlHazard;
        private readonly MachineState _state;

        public PipelineController(MachineState state, string predictorType = "static")
        {
            _state = state;
            _buffers = new PipelineBuffers();
            Predictor = PredictorFactory.Create(predictorType);

            _fetch = new FetchStage(state);
            _decode = new DecodeStage(state);
            _execute = new ExecuteStage(state);
            _memory = new MemoryStage(state);
            _writeback = new WritebackStage(state);

            _dataHazard = new DataHazardUnit();
            _structHazard = new StructuralHazardUnit();
            _controlHazard = new ControlHazardUnit();
        }

        public void Cycle()
        {
            // 1. Resolve Data/Structural Hazards
            _dataHazard.Resolve(_buffers);
            _structHazard.DetectAndHandle(_buffers);
            
            // 2. Commit (Writeback)
            _writeback.Run(_buffers);
            if (_state.Halted) { _buffers.Flush(); return; }

            // 3. Memory Stage
            _memory.Run(_buffers);

            // 4. Execute Stage (Calculates Misprediction)
            _execute.Run(_buffers); 

            // --- LEARNING STEP ---
            // If the Execute stage processed a branch/jump, update the predictor.
            if (!_buffers.ExecuteMemory.IsEmpty && _buffers.ExecuteMemory.DecodedInst != null)
            {
                var inst = _buffers.ExecuteMemory.DecodedInst;
                if (inst.IsBranch || inst.IsJump)
                {
                    // Feed the actual results back into the predictor so it learns
                    Predictor.Update(
                        _buffers.ExecuteMemory.PC, 
                        _buffers.ExecuteMemory.BranchTaken, 
                        _buffers.ExecuteMemory.ActualTarget
                    );
                }
            }
            // ---------------------

            // 5. Control Hazards (Flush on Misprediction)
            // If Execute said "Misprediction", we flush now.
            if (_controlHazard.DetectAndHandle(_buffers))
            {
                // Flushed logic handles cleanup
            }

            // 6. Front End
            _decode.Run(_buffers);
            _fetch.Run(_buffers, Predictor); 
        }
    }
}
