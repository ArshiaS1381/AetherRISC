using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Stages;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors;
using AetherRISC.Core.Abstractions.Diagnostics;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Controller
{
    public class PipelineController
    {
        public PipelineBuffers Buffers { get; }
        public IBranchPredictor Predictor { get; }
        
        // Public Metrics for the Runner to read
        public PerformanceMetrics Metrics { get; } = new PerformanceMetrics();

        private readonly FetchStage _fetch;
        private readonly DecodeStage _decode;
        private readonly ExecuteStage _execute;
        private readonly MemoryStage _memory;
        private readonly WritebackStage _writeback;

        private readonly DataHazardUnit _dataHazard;
        private readonly StructuralHazardUnit _structHazard;
        private readonly ControlHazardUnit _controlHazard;
        private readonly MachineState _state;
        private readonly ArchitectureSettings _settings;

        public PipelineController(MachineState state, string predictorType, ArchitectureSettings settings)
        {
            _state = state;
            _settings = settings ?? new ArchitectureSettings();
            Buffers = new PipelineBuffers();
            Predictor = PredictorFactory.Create(predictorType, _settings);

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
            // [METRICS] Cycle Count
            Metrics.TotalCycles++;
            ulong pcAtStartOfCycle = _state.Registers.PC;

            // 1. Resolve Hazards
            _dataHazard.Resolve(Buffers);
            _structHazard.DetectAndHandle(Buffers);
            
            // [METRICS] Stall Counting (FIXED: Only check FetchDecode)
            // If the front-end is stalled, the whole pipeline is effectively stalling for data.
            if (Buffers.FetchDecode.IsStalled)
            {
                Metrics.DataHazardStalls++;
            }

            // 2. Commit (Writeback)
            _writeback.Run(Buffers);
            
            // [METRICS] Retired Instruction Counting
            if (!Buffers.MemoryWriteback.IsEmpty) Metrics.InstructionsRetired++;

            if (_state.Halted) { Buffers.Flush(); return; }

            // 3. Memory
            _memory.Run(Buffers);

            // 4. Execute
            _execute.Run(Buffers); 
            ulong pcAfterExecute = _state.Registers.PC;
            bool executeChangedPC = (pcAfterExecute != pcAtStartOfCycle);

            // [METRICS] Branch/Jump Analysis
            if (!Buffers.ExecuteMemory.IsEmpty && Buffers.ExecuteMemory.DecodedInst != null)
            {
                var inst = Buffers.ExecuteMemory.DecodedInst;
                bool isMisprediction = Buffers.ExecuteMemory.Misprediction;

                if (inst.IsBranch) // Conditional
                {
                    Metrics.TotalBranches++;
                    if (isMisprediction) Metrics.BranchMisses++;
                    else Metrics.BranchHits++;
                    
                    Predictor.Update(Buffers.ExecuteMemory.PC, Buffers.ExecuteMemory.BranchTaken, Buffers.ExecuteMemory.ActualTarget);
                }
                else if (inst.IsJump) // Unconditional
                {
                    Metrics.TotalJumps++;
                    Predictor.Update(Buffers.ExecuteMemory.PC, true, Buffers.ExecuteMemory.ActualTarget);
                }
            }

            // 5. Control Hazards
            bool mispredictionOccurred = _controlHazard.DetectAndHandle(Buffers);
            
            // [METRICS] Flush Counting
            if (mispredictionOccurred) Metrics.ControlHazardFlushes++;

            // 6. Decode
            _decode.Run(Buffers);
            
            // 7. Fetch Logic (Re-applying the Cycle Accuracy Fix)
            if (_settings.EnableEarlyBranchResolution)
            {
                // [FAST MODE] 1 Cycle Penalty
                _fetch.Run(Buffers, Predictor); 
            }
            else
            {
                // [REALISTIC MODE] 2 Cycle Penalty
                // Force Fetch to use the PC from start of cycle
                _state.Registers.PC = pcAtStartOfCycle;
                _fetch.Run(Buffers, Predictor);
                
                // If Execute actually jumped, it overrides Fetch
                if (executeChangedPC)
                {
                    _state.Registers.PC = pcAfterExecute;
                    Buffers.FetchDecode.Flush();
                }
            }
        }
    }
}
