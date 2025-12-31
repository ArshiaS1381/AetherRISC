using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Stages;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Memory; // For IMemoryBus

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Controller
{
    public class PipelineController
    {
        public PipelineBuffers Buffers { get; }
        public IBranchPredictor Predictor { get; }
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

        public PipelineController(MachineState state, ArchitectureSettings settings)
        {
            _state = state;
            Buffers = new PipelineBuffers(settings);
            Predictor = PredictorFactory.Create(settings);
            Metrics.PipelineWidth = settings.PipelineWidth;
            
            // Re-attach memory to inject the Metrics into the CachedMemoryBus
            if(state.Memory != null && state.Memory is not AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy.CachedMemoryBus)
            {
                // This is a bit of a hack to re-wrap physical RAM if it was already set simple
                // Ideally, MachineState setup happens before Controller
                state.AttachMemory(state.Memory, Metrics);
            }
            else if (state.Memory != null && settings.EnableCacheSimulation)
            {
                // If the memory bus is generic but cache sim enabled, re-wrap it using our metrics
                // This assumes state.Memory is the underlying RAM
                state.AttachMemory(state.Memory, Metrics);
            }
            
            _fetch = new FetchStage(state, settings);
            _decode = new DecodeStage(state, settings);
            _execute = new ExecuteStage(state, settings);
            _memory = new MemoryStage(state);
            _writeback = new WritebackStage(state);
            
            _dataHazard = new DataHazardUnit { StateContext = state, Settings = settings };
            _structHazard = new StructuralHazardUnit();
            _controlHazard = new ControlHazardUnit();
        }

        public void Cycle()
        {
            Metrics.TotalCycles++;
            Buffers.ResetStalls();

            _dataHazard.Resolve(Buffers);
            _structHazard.DetectAndHandle(Buffers);
            if (Buffers.FetchDecode.IsStalled) Metrics.DataHazardStalls++;

            _writeback.Run(Buffers);
            
            for(int i=0; i<Buffers.Width; i++) 
            {
                var slot = Buffers.MemoryWriteback.Slots[i];
                if (_state.Halted) break;
                
                if (slot.Valid && !slot.IsBubble) 
                {
                    Metrics.InstructionsRetired++;
                    bool isFused = slot.DecodedInst?.GetType().Namespace?.Contains("Pseudo") ?? false;
                    Metrics.IsaInstructionsRetired += (ulong)(isFused ? 2 : 1);
                }
                slot.Reset();
            }

            if (_state.Halted) { Buffers.FlushAll(); return; }

            _memory.Run(Buffers);
            _execute.Run(Buffers);
            
            for(int i=0; i<Buffers.Width; i++)
            {
                var op = Buffers.ExecuteMemory.Slots[i];
                if(op.Valid && !op.IsBubble && op.DecodedInst != null && (op.DecodedInst.IsBranch || op.DecodedInst.IsJump)) {
                    Metrics.TotalBranches++;
                    if(op.Misprediction) Metrics.BranchMisses++; else Metrics.BranchHits++;
                    Predictor.Update(op.PC, op.BranchTaken, op.ActualTarget);
                }
            }

            if (_controlHazard.DetectAndHandle(Buffers)) Metrics.ControlHazardFlushes++;

            _decode.Run(Buffers);
            _fetch.Run(Buffers, Predictor); 
        }
    }
}
