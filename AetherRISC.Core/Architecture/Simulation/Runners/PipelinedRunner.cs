using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

namespace AetherRISC.Core.Architecture.Simulation.Runners
{
    public class PipelinedRunner 
    {
        private readonly MachineState _state;
        private readonly ISimulationLogger _logger;
        private readonly PipelineController _controller;
        private readonly InstructionDecoder _visDecoder;

        public PipelineBuffers PipelineState => _controller.Buffers;

        // Constructor now takes the predictor type
        public PipelinedRunner(MachineState state, ISimulationLogger logger, string branchPredictor = "static")
        {
            _state = state;
            _logger = logger;
            _controller = new PipelineController(state, branchPredictor);
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

        public void Step(int cycleCount) => StepInternal(cycleCount);

        private void StepInternal(int cycleIndex)
        {
            if (_logger.IsVerbose) { _logger.BeginCycle(cycleIndex); LogPipelineStatus(); }

            try { _controller.Cycle(); }
            catch (Exception ex)
            {
                _logger.Log("ERR", $"Pipeline Fault: {ex.Message}");
                _state.Halted = true;
            }

            if (_logger.IsVerbose) _logger.CompleteCycle();
        }

        private void LogPipelineStatus()
        {
            var b = _controller.Buffers;

            // ... (Existing logging logic) ...
            
            if (b.FetchDecode.IsStalled)
            {
                _logger.Log("ID", "** STALLED **");
            }
            else if (!b.FetchDecode.IsEmpty)
            {
                // Visual Decode
                var inst = _visDecoder.Decode(b.FetchDecode.Instruction);
                string predInfo = b.FetchDecode.PredictedTaken ? $" [P:TAKEN 0x{b.FetchDecode.PredictedTarget:X}]" : "";
                if (inst != null) _logger.LogStageDecode(b.FetchDecode.PC, b.FetchDecode.Instruction, inst);
                if (b.FetchDecode.PredictedTaken) _logger.Log("ID", predInfo);
            }
        }
    }
}
