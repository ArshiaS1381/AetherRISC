using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Abstractions.Diagnostics;

namespace AetherRISC.Core.Architecture.Simulation.Runners
{
    public class PipelinedRunner 
    {
        private readonly MachineState _state;
        private readonly ISimulationLogger _logger;
        private readonly PipelineController _controller;
        
        private readonly InstructionDecoder _visDecoder;

        public PipelineBuffers PipelineState => _controller.Buffers;
        public PerformanceMetrics Metrics => _controller.Metrics;
        public IBranchPredictor Predictor => _controller.Predictor;

        public PipelinedRunner(MachineState state, ISimulationLogger logger, ArchitectureSettings settings)
        {
            _state = state;
            _logger = logger;
            // FIXED: Removed the old "branchPredictor" string argument
            _controller = new PipelineController(state, settings);
            _visDecoder = new InstructionDecoder();
        }

        public void Run(int maxCycles = -1)
        {
            _logger.Initialize("CLI_Pipeline_Simulation");
            int cycles = 0;

            while (!_state.Halted)
            {
                if (maxCycles != -1 && cycles >= maxCycles) break;
                StepInternal(cycles);
                cycles++;
            }
            _logger.FinalizeSession();
        }

        public void Step(int cycleCount) { for(int i=0; i<cycleCount; i++) StepInternal(i); }

        private void StepInternal(int cycleIndex)
        {
            if (_logger.IsVerbose) { _logger.BeginCycle(cycleIndex); LogPipelineStatus(); }

            try { _controller.Cycle(); }
            catch (Exception ex)
            {
                _logger.Log("ERR", $@"Pipeline Fault: {ex.Message}");
                _state.Halted = true;
            }

            if (_logger.IsVerbose) _logger.CompleteCycle();
        }

        private void LogPipelineStatus()
        {
            var b = _controller.Buffers;
            if (b.FetchDecode.IsStalled)
            {
                _logger.Log("ID", "** STALLED **");
            }
            else
            {
                for(int i=0; i<b.Width; i++)
                {
                     var op = b.FetchDecode.Slots[i];
                     if(op.Valid) 
                     {
                         var inst = _visDecoder.Decode(op.RawInstruction);
                         string pred = op.PredictedTaken ? " [P:TAKEN]" : "";
                         
                         if (inst != null)
                            _logger.LogStageDecode(op.PC, op.RawInstruction, inst);
                         else 
                            _logger.Log("ID", $@"Unknown/Bubble @{op.PC:X}");

                         if (op.PredictedTaken) _logger.Log("ID", $@" {pred} @{op.PredictedTarget:X}");
                     }
                }
            }
        }
    }
}
