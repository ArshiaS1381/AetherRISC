using AetherRISC.Core.Abstractions.Interfaces;
using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline
{
    public class PipelineMicroOp
    {
        public bool Valid { get; set; } = false;
        public bool IsBubble { get; set; } = false;

        public ulong PC { get; set; }
        public uint RawInstruction { get; set; }
        public IInstruction? DecodedInst { get; set; }
        
        public bool PredictedTaken { get; set; }
        public ulong PredictedTarget { get; set; }

        public int Rd { get; set; }
        public int Rs1 { get; set; }
        public int Rs2 { get; set; }

        public int Immediate { get; set; }
        
        public bool RegWrite { get; set; }
        public bool IsFloatRegWrite { get; set; }
        
        public bool MemRead { get; set; }
        public bool MemWrite { get; set; }

        public ulong? ForwardedRs1 { get; set; }
        public ulong? ForwardedRs2 { get; set; }

        public ulong AluResult { get; set; }
        public ulong StoreValue { get; set; }

        public bool BranchTaken { get; set; }
        public bool Misprediction { get; set; }
        public ulong CorrectTarget { get; set; }
        public ulong ActualTarget { get; set; }
        public ulong FinalResult { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Valid = false;
            IsBubble = false;
            
            // FIX: Set to NOP (ADDI x0, x0, 0) so visualizers don't see FFFFFFFF or EBREAK
            // 0x13 is proper RISC-V NOP.
            RawInstruction = 0x00000013; 
            DecodedInst = null;
            PC = 0;
            
            PredictedTaken = false;
            BranchTaken = false;
            Misprediction = false;
            
            RegWrite = false;
            MemRead = false;
            MemWrite = false;
            IsFloatRegWrite = false;
            
            Rd = 0;
            Rs1 = 0;
            Rs2 = 0;
            
            ForwardedRs1 = null;
            ForwardedRs2 = null;
        }
    }
}
