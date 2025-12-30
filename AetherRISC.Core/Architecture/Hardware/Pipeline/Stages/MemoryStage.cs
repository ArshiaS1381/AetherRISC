using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Abstractions.Interfaces;

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
            if (!buffers.MemoryWriteback.IsStalled)
            {
                buffers.MemoryWriteback.Flush();
            }

            if (buffers.ExecuteMemory.IsEmpty) return;
            
            // Check for memory once. If null, we can't do anything useful.
            IMemoryBus? mem = _state.Memory;
            if (mem == null) return;

            buffers.MemoryWriteback.SetHasContent();

            var inputs = buffers.ExecuteMemory.Slots;
            var outputs = buffers.MemoryWriteback.Slots;
            int width = buffers.Width;

            for (int i = 0; i < width; i++)
            {
                var input = inputs[i];
                var output = outputs[i];

                if (!input.Valid || input.IsBubble) { output.Reset(); continue; }

                output.Valid = true;
                output.DecodedInst = input.DecodedInst;
                output.RawInstruction = input.RawInstruction;
                output.PC = input.PC;
                output.Rd = input.Rd;
                output.RegWrite = input.RegWrite;
                output.IsFloatRegWrite = input.IsFloatRegWrite;

                ulong result = input.AluResult;
                uint addr = (uint)input.AluResult;
                
                uint raw = input.RawInstruction;
                uint opcode = raw & 0x7F;
                uint funct3 = (raw >> 12) & 0x7;

                if (input.MemRead)
                {
                    if (opcode == 0x03) 
                    {
                        switch (funct3)
                        {
                            case 0: byte b = mem.ReadByte(addr); result = (ulong)(long)(sbyte)b; break;
                            case 1: ushort h = mem.ReadHalf(addr); result = (ulong)(long)(short)h; break;
                            case 2: uint w = mem.ReadWord(addr); result = (ulong)(long)(int)w; break;
                            case 3: result = mem.ReadDouble(addr); break;
                            case 4: result = (ulong)mem.ReadByte(addr); break;
                            case 5: result = (ulong)mem.ReadHalf(addr); break;
                            case 6: result = (ulong)mem.ReadWord(addr); break;
                        }
                    }
                    else if (opcode == 0x07)
                    {
                         if (funct3 == 2) {
                             uint w = mem.ReadWord(addr);
                             result = 0xFFFFFFFF00000000 | (ulong)w; 
                         }
                         else if (funct3 == 3) result = mem.ReadDouble(addr);
                    }
                }
                
                if (input.MemWrite)
                {
                    if (opcode == 0x23)
                    {
                        switch (funct3)
                        {
                            case 0: mem.WriteByte(addr, (byte)input.StoreValue); break; 
                            case 1: mem.WriteHalf(addr, (ushort)input.StoreValue); break; 
                            case 2: mem.WriteWord(addr, (uint)input.StoreValue); break; 
                            case 3: mem.WriteDouble(addr, input.StoreValue); break; 
                        }
                    }
                    else if (opcode == 0x27)
                    {
                        if (funct3 == 2) mem.WriteWord(addr, (uint)input.StoreValue); 
                        else if (funct3 == 3) mem.WriteDouble(addr, input.StoreValue); 
                    }
                }
                
                if (!input.MemRead)
                {
                     if (opcode == 0x2F) result = input.FinalResult;
                     else result = input.AluResult;
                }

                output.FinalResult = result;
            }
        }
    }
}
