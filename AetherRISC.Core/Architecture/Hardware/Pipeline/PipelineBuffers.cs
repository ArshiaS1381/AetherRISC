using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline
{
    public class PipelineBuffers
    {
        public int Width { get; }
        public int FetchWidth { get; }

        public PipelineStageBuffer FetchDecode { get; }
        public PipelineStageBuffer DecodeExecute { get; }
        public PipelineStageBuffer ExecuteMemory { get; }
        public PipelineStageBuffer MemoryWriteback { get; }

        public PipelineBuffers(ArchitectureSettings settings)
        {
            Width = settings.PipelineWidth;
            FetchWidth = (int)Math.Ceiling(Width * settings.FetchBufferRatio);

            FetchDecode = new PipelineStageBuffer(FetchWidth);
            DecodeExecute = new PipelineStageBuffer(Width);
            ExecuteMemory = new PipelineStageBuffer(Width);
            MemoryWriteback = new PipelineStageBuffer(Width);
        }

        public void FlushAll()
        {
            FetchDecode.Flush();
            DecodeExecute.Flush();
            ExecuteMemory.Flush();
            MemoryWriteback.Flush();
        }

        public void ResetStalls()
        {
            FetchDecode.IsStalled = false;
            DecodeExecute.IsStalled = false;
            ExecuteMemory.IsStalled = false;
            MemoryWriteback.IsStalled = false;
        }
    }

    public class PipelineStageBuffer
    {
        public PipelineMicroOp[] Slots { get; }
        public bool IsStalled { get; set; }
        public bool IsEmpty { get; private set; } = true;

        public PipelineStageBuffer(int width)
        {
            Slots = new PipelineMicroOp[width];
            for (int i = 0; i < width; i++) Slots[i] = new PipelineMicroOp();
        }

        public void Flush()
        {
            IsStalled = false;
            IsEmpty = true;
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i].Reset();
            }
        }
        
        public void SetHasContent() => IsEmpty = false;
    }
}
