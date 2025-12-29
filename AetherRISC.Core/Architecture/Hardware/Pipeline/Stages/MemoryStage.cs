using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class MemoryStage
    {
        private readonly MachineState _state;

        public MemoryStage(MachineState state)
        {
            _state = state;
        }

        public void Run(PipelineBuffers buffers)
        {
            if (buffers.ExecuteMemory.IsEmpty)
            {
                buffers.MemoryWriteback.IsEmpty = true;
                return;
            }

            // Safety check for uninitialized memory
            if (_state.Memory == null)
                throw new InvalidOperationException("Memory subsystem not initialized");

            var input = buffers.ExecuteMemory;
            var output = buffers.MemoryWriteback;

            output.DecodedInst = input.DecodedInst;
            output.RawInstruction = input.RawInstruction;
            output.PC = input.PC;
            output.Rd = input.Rd;
            output.RegWrite = input.RegWrite;

            ulong result = input.AluResult;
            uint addr = (uint)input.AluResult;

            if (input.MemRead && input.DecodedInst != null)
            {
                var name = input.DecodedInst.GetType().Name.ToUpper().Replace("INSTRUCTION", "");

                if (name == "LB")       result = (ulong)(long)(sbyte)_state.Memory.ReadByte(addr);
                else if (name == "LBU") result = (ulong)_state.Memory.ReadByte(addr);
                else if (name == "LH")  result = (ulong)(long)(short)_state.Memory.ReadHalf(addr);
                else if (name == "LHU") result = (ulong)_state.Memory.ReadHalf(addr);
                else if (name == "LW")  result = (ulong)(long)(int)_state.Memory.ReadWord(addr);
                else if (name == "LWU") result = (ulong)_state.Memory.ReadWord(addr);
                else result = (ulong)_state.Memory.ReadWord(addr); 
            }
            else if (input.MemWrite && input.DecodedInst != null)
            {
                var name = input.DecodedInst.GetType().Name.ToUpper().Replace("INSTRUCTION", "");

                if (name == "SB")      _state.Memory.WriteByte(addr, (byte)input.StoreValue);
                else if (name == "SH") _state.Memory.WriteHalf(addr, (ushort)input.StoreValue);
                else _state.Memory.WriteWord(addr, (uint)input.StoreValue);
            }
            
            output.FinalResult = result;
            output.IsEmpty = false;
        }
    }
}
