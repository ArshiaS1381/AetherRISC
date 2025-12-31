using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.Simulation.Runners
{
    public class PipelinedRunner : ISimulationRunner
    {
        private readonly MachineState _state;
        private readonly ISimulationLogger _logger;
        private readonly PipelineController _controller;
        
        public PipelineBuffers PipelineState => _controller.Buffers;
        public PerformanceMetrics Metrics => _controller.Metrics;
        public IBranchPredictor Predictor => _controller.Predictor;

        public PipelinedRunner(MachineState state, ISimulationLogger logger, ArchitectureSettings settings)
        {
            _state = state;
            _logger = logger;
            _controller = new PipelineController(state, settings);
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
            if (_logger.IsVerbose) _logger.BeginCycle(cycleIndex);

            try { _controller.Cycle(); }
            catch (Exception ex)
            {
                _logger.Log("ERR", $@"Pipeline Fault: {ex.Message}");
                _state.Halted = true;
            }

            if (_logger.IsVerbose) _logger.CompleteCycle();
        }
    }
}
