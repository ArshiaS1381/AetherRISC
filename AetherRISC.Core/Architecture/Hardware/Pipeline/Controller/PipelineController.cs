using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Stages;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.ISA.Pseudo;

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

        public PipelineController(MachineState state, string predictorType, ArchitectureSettings settings)
        {
            _state = state;
            Buffers = new PipelineBuffers(settings);
            Predictor = PredictorFactory.Create(predictorType, settings);
            Metrics.PipelineWidth = settings.PipelineWidth;
            
            _fetch = new FetchStage(state, settings);
            _decode = new DecodeStage(state, settings);
            _execute = new ExecuteStage(state, settings);
            _memory = new MemoryStage(state);
            _writeback = new WritebackStage(state);
            
            _dataHazard = new DataHazardUnit();
            _dataHazard.StateContext = state; 
            _dataHazard.Settings = settings;

            _structHazard = new StructuralHazardUnit();
            _controlHazard = new ControlHazardUnit();
        }

        public void Cycle()
        {
            Metrics.TotalCycles++;
            Buffers.ResetStalls();

            SimpleProfiler.Start(SimpleProfiler.Hazard_Data);
            _dataHazard.Resolve(Buffers);
            SimpleProfiler.Stop(SimpleProfiler.Hazard_Data);

            SimpleProfiler.Start(SimpleProfiler.Hazard_Struct);
            _structHazard.DetectAndHandle(Buffers);
            if (Buffers.FetchDecode.IsStalled) Metrics.DataHazardStalls++;
            SimpleProfiler.Stop(SimpleProfiler.Hazard_Struct);

            SimpleProfiler.Start(SimpleProfiler.Stage_WB);
            _writeback.Run(Buffers);
            
            // --- Retirement Logic ---
            for(int i=0; i<Buffers.Width; i++) 
            {
                var slot = Buffers.MemoryWriteback.Slots[i];
                if (_state.Halted) break;
                
                if (slot.Valid && !slot.IsBubble) 
                {
                    Metrics.InstructionsRetired++;
                    
                    // Check for Fusion to calculate Real ISA count
                    if (slot.DecodedInst is FusedComputationalInstruction || 
                        slot.DecodedInst is FusedLoadInstruction)
                    {
                        Metrics.IsaInstructionsRetired += 2; // Fused ops represent 2 ISA insts
                    }
                    else
                    {
                        Metrics.IsaInstructionsRetired += 1;
                    }
                }
                slot.Reset();
            }
            SimpleProfiler.Stop(SimpleProfiler.Stage_WB);

            if (_state.Halted) { Buffers.FlushAll(); return; }

            SimpleProfiler.Start(SimpleProfiler.Stage_Mem);
            _memory.Run(Buffers);
            SimpleProfiler.Stop(SimpleProfiler.Stage_Mem);

            SimpleProfiler.Start(SimpleProfiler.Stage_Ex);
            _execute.Run(Buffers);
            for(int i=0; i<Buffers.Width; i++)
            {
                var op = Buffers.ExecuteMemory.Slots[i];
                if(op.Valid && !op.IsBubble && op.DecodedInst != null && op.DecodedInst.IsBranch) {
                    Metrics.TotalBranches++;
                    if(op.Misprediction) Metrics.BranchMisses++; else Metrics.BranchHits++;
                    Predictor.Update(op.PC, op.BranchTaken, op.ActualTarget);
                }
            }
            SimpleProfiler.Stop(SimpleProfiler.Stage_Ex);

            SimpleProfiler.Start(SimpleProfiler.Hazard_Ctrl);
            if (_controlHazard.DetectAndHandle(Buffers)) Metrics.ControlHazardFlushes++;
            SimpleProfiler.Stop(SimpleProfiler.Hazard_Ctrl);

            SimpleProfiler.Start(SimpleProfiler.Stage_Dec);
            _decode.Run(Buffers);
            SimpleProfiler.Stop(SimpleProfiler.Stage_Dec);
            
            SimpleProfiler.Start(SimpleProfiler.Stage_Fetch);
            _fetch.Run(Buffers, Predictor); 
            SimpleProfiler.Stop(SimpleProfiler.Stage_Fetch);
        }
    }
}
