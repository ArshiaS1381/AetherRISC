using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Pseudo;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class DecodeStage
    {
        private readonly MachineState _state;
        private readonly InstructionDecoder _decoder;
        private readonly IInstruction _implicitNop;
        private readonly ArchitectureSettings _settings;

        // Fix: Make settings nullable in signature
        public DecodeStage(MachineState state, ArchitectureSettings? settings = null)
        {
            _state = state;
            _decoder = new InstructionDecoder();
            _implicitNop = new AddiInstruction(0, 0, 0);
            _settings = settings ?? new ArchitectureSettings();
        }

        public void Run(PipelineBuffers buffers)
        {
            if (buffers.DecodeExecute.IsStalled)
            {
                buffers.FetchDecode.IsStalled = true;
                return;
            }

            buffers.DecodeExecute.Flush();
            
            if (buffers.FetchDecode.IsStalled || buffers.FetchDecode.IsEmpty) return;

            buffers.DecodeExecute.SetHasContent();
            
            var inputs = buffers.FetchDecode.Slots; 
            var outputs = buffers.DecodeExecute.Slots;
            
            int inputCount = inputs.Length;
            int outputCapacity = outputs.Length;
            int writeIdx = 0; 
            
            int i = 0; 

            for (; i < inputCount; i++)
            {
                if (writeIdx >= outputCapacity) break;

                var input = inputs[i];
                if (!input.Valid || input.IsBubble) continue;

                var output = outputs[writeIdx];
                bool fused = false;

                if (_settings.EnableMacroOpFusion && i < inputCount - 1)
                {
                    var next = inputs[i + 1];
                    if (next.Valid && !next.IsBubble)
                    {
                        if (TryFuse(input, next, output))
                        {
                            i++; 
                            writeIdx++;
                            fused = true;
                        }
                    }
                }

                if (fused) continue;

                DecodeSingle(input, output);
                writeIdx++;
            }

            if (i < inputCount)
            {
                for (; i < inputCount; i++)
                {
                    var unconsumed = inputs[i];
                    if (unconsumed.Valid && !unconsumed.IsBubble)
                    {
                        _state.ProgramCounter = unconsumed.PC;
                        break; 
                    }
                }
            }
        }

        private bool TryFuse(PipelineMicroOp input, PipelineMicroOp next, PipelineMicroOp output)
        {
            uint rawA = input.RawInstruction;
            uint rawB = next.RawInstruction;
            int opcodeA = (int)(rawA & 0x7F);
            int rdA = (int)((rawA >> 7) & 0x1F);
            int uImm = (int)(rawA & 0xFFFFF000); 

            bool isLui = opcodeA == 0x37;
            bool isAuipc = opcodeA == 0x17;

            if ((isLui || isAuipc) && rdA != 0)
            {
                int opcodeB = (int)(rawB & 0x7F);
                int rdB = (int)((rawB >> 7) & 0x1F);
                int rs1B = (int)((rawB >> 15) & 0x1F);
                
                if (rs1B == rdA && rdB == rdA)
                {
                    int imm12B = (int)(rawB >> 20); 
                    if ((imm12B & 0x800) != 0) imm12B |= unchecked((int)0xFFFFF000);

                    if (opcodeB == 0x13 && ((rawB >> 12) & 0x7) == 0) 
                    {
                        long baseVal = isLui ? 0 : (long)input.PC;
                        long finalVal = baseVal + (long)uImm + (long)imm12B;
                        var fusedInst = new FusedComputationalInstruction(isLui ? "LI (Fused)" : "LA (Fused)", rdA, finalVal);
                        SetupFusedOutput(output, next, fusedInst, finalVal, rawB); 
                        return true;
                    }
                    else if (opcodeB == 0x03) 
                    {
                        long baseVal = isLui ? 0 : (long)input.PC;
                        long addr = baseVal + (long)uImm + (long)imm12B;
                        int funct3 = (int)((rawB >> 12) & 0x7);
                        int bytes = (funct3 & 3) == 0 ? 1 : ((funct3 & 3) == 1 ? 2 : ((funct3 & 3) == 2 ? 4 : 8));
                        bool signed = (funct3 < 4) && (funct3 != 3) && (funct3 != 6);
                        var fusedInst = new FusedLoadInstruction(isLui ? "L[x]Abs (Fused)" : "L[x]PC (Fused)", rdA, addr, bytes, signed);
                        SetupFusedOutput(output, next, fusedInst, addr, rawB);
                        output.MemRead = true;
                        return true;
                    }
                }
            }
            return false;
        }

        private void DecodeSingle(PipelineMicroOp input, PipelineMicroOp output)
        {
            var cached = _decoder.DecodeFast(input.RawInstruction);
            IInstruction inst;

            if (cached != null)
            {
                inst = cached.Inst;
                output.Rd = cached.Rd; output.Rs1 = cached.Rs1; output.Rs2 = cached.Rs2;
                output.Immediate = cached.Imm; output.MemRead = cached.IsLoad; output.MemWrite = cached.IsStore;
                output.IsFloatRegWrite = cached.IsFloatRegWrite; output.RegWrite = cached.RegWrite;
            }
            else
            {
                inst = _implicitNop;
                output.Rd = 0; output.Rs1 = 0; output.Rs2 = 0; output.Immediate = 0;
                output.MemRead = false; output.MemWrite = false; output.IsFloatRegWrite = false; output.RegWrite = false;
            }

            output.Valid = true;
            output.DecodedInst = inst;
            output.RawInstruction = input.RawInstruction;
            output.PC = input.PC;
            output.PredictedTaken = input.PredictedTaken;
            output.PredictedTarget = input.PredictedTarget;
            output.ForwardedRs1 = null; output.ForwardedRs2 = null;
        }

        private void SetupFusedOutput(PipelineMicroOp outOp, PipelineMicroOp nextIn, IInstruction inst, long result, uint rawB)
        {
            outOp.Valid = true;
            outOp.DecodedInst = inst;
            outOp.RawInstruction = rawB; 
            outOp.PC = nextIn.PC; 
            outOp.Rd = inst.Rd; outOp.Rs1 = 0; outOp.Rs2 = 0;
            outOp.Immediate = inst.Imm;
            outOp.RegWrite = true;
            outOp.PredictedTaken = nextIn.PredictedTaken; 
            outOp.PredictedTarget = nextIn.PredictedTarget;
        }
    }
}
