using System;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Stages;
using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Controller
{
    public class PipelineController
    {
        private readonly PipelineBuffers _buffers;
        public PipelineBuffers Buffers => _buffers;

        private readonly FetchStage _fetch;
        private readonly DecodeStage _decode;
        private readonly ExecuteStage _execute;
        private readonly MemoryStage _memory;
        private readonly WritebackStage _writeback;

        private readonly DataHazardUnit _dataHazard;
        private readonly StructuralHazardUnit _structHazard;

        private readonly MachineState _state;

        public PipelineController(MachineState state)
        {
            _state = state;
            _buffers = new PipelineBuffers();

            _fetch = new FetchStage(state);
            _decode = new DecodeStage(state);
            _execute = new ExecuteStage(state);
            _memory = new MemoryStage(state);
            _writeback = new WritebackStage(state);

            _dataHazard = new DataHazardUnit();
            _structHazard = new StructuralHazardUnit();
        }

        public void Cycle()
        {
            // 1. Resolve Hazards based on current buffer state
            _dataHazard.Resolve(_buffers);
            _structHazard.DetectAndHandle(_buffers);

            // 2. Commit Stage (Writeback)
            // This is where EBREAK sets _state.Halted = true
            _writeback.Run(_buffers);

            // CRITICAL FIX: 
            // If the machine halted this cycle (e.g., EBREAK retired), we must
            // immediately flush the pipeline to kill any younger instructions
            // (like the ADDI 73) that are currently in Fetch/Decode/Execute.
            // We also return immediately to stop them from advancing.
            if (_state.Halted)
            {
                _buffers.Flush();
                return;
            }

            // 3. Run remaining stages in reverse order
            _memory.Run(_buffers);
            _execute.Run(_buffers);

            // 4. Handle Control Hazards (Branch Misprediction)
            // If Execute produced a branch, flush the younger stages immediately
            if (!_buffers.ExecuteMemory.IsEmpty && _buffers.ExecuteMemory.BranchTaken)
            {
                _buffers.DecodeExecute.Flush();
                _buffers.FetchDecode.Flush();
            }

            // 5. Run Front-End
            _decode.Run(_buffers);
            _fetch.Run(_buffers);
        }

        public void Flush()
        {
            _buffers.Flush();
        }
    }
}
