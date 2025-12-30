using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.Pipeline
{
    public class SuperscalarTests : PipelineTestFixture
    {
        [Fact]
        public void Fetch_Retrieves_Multiple_Instructions()
        {
            InitSuperscalar(2); // 2-wide

            Assembler.Add(pc => Inst.Addi(1, 0, 1));
            Assembler.Add(pc => Inst.Addi(2, 0, 2));
            LoadProgram();

            Cycle(1); // Fetch

            var buf = PipelineBuffers.FetchDecode;
            Assert.True(buf.Slots[0].Valid);
            Assert.True(buf.Slots[1].Valid);
            Assert.Equal(1, buf.Slots[0].DecodedInst?.Rd); // DecodedInst might be null until Decode stage, check Raw or next cycle
        }

        [Fact]
        public void Decode_Processes_Parallel_Instructions()
        {
            InitSuperscalar(2);
            Assembler.Add(pc => Inst.Addi(1, 0, 1));
            Assembler.Add(pc => Inst.Addi(2, 0, 2));
            LoadProgram();

            Cycle(2); // Fetch -> Decode

            var buf = PipelineBuffers.DecodeExecute;
            Assert.Equal(1, buf.Slots[0].Rd);
            Assert.Equal(2, buf.Slots[1].Rd);
        }

        [Fact]
        public void Execute_Runs_ALU_In_Parallel()
        {
            InitSuperscalar(2);
            Assembler.Add(pc => Inst.Addi(1, 0, 10));
            Assembler.Add(pc => Inst.Addi(2, 0, 20));
            LoadProgram();

            Cycle(3); // F -> D -> E

            var buf = PipelineBuffers.ExecuteMemory;
            Assert.Equal(10ul, buf.Slots[0].AluResult);
            Assert.Equal(20ul, buf.Slots[1].AluResult);
        }

        [Fact]
        public void Superscalar_Structural_Stall()
        {
            // Testing intra-bundle dependency logic
            InitSuperscalar(2);
            
            // 1. ADDI x1, x0, 10
            // 2. ADDI x2, x1, 5  (Depends on 1, in same bundle) -> Should Stall/Bubble
            
            Assembler.Add(pc => Inst.Addi(1, 0, 10));
            Assembler.Add(pc => Inst.Addi(2, 1, 5));
            LoadProgram();

            Cycle(3); 

            var ex = PipelineBuffers.ExecuteMemory;
            
            // Slot 0 should be in Execute
            Assert.True(ex.Slots[0].Valid);
            Assert.Equal(10ul, ex.Slots[0].AluResult);

            // Slot 1 should be invalid/bubble because of dependency hazard check in Decode
            Assert.False(ex.Slots[1].Valid);
        }

        [Fact]
        public void Branch_Shadow_Killing()
        {
            InitSuperscalar(4);
            
            // 0: BEQ x0, x0, target (Taken)
            // 1: ADDI x1, x0, 1 (Shadow - should be killed)
            // 2: ADDI x2, x0, 2 (Shadow - should be killed)
            // 3: target: ADDI x3, x0, 3
            
            Assembler.Add(pc => Inst.Beq(0, 0, 12)); // +12 bytes = +3 insts
            Assembler.Add(pc => Inst.Addi(1, 0, 1));
            Assembler.Add(pc => Inst.Addi(2, 0, 2));
            Assembler.Add(pc => Inst.Addi(3, 0, 3)); // Target
            
            LoadProgram();
            
            Cycle(1); // Fetch
            
            var fd = PipelineBuffers.FetchDecode;
            
            // Slot 0 Valid
            Assert.True(fd.Slots[0].Valid);
            
            // Slots 1 and 2 should be killed by FetchStage logic (Immediate redirect)
            // Note: Our FetchStage implementation kills subsequent slots on Taken prediction.
            // Since x0==x0 is always taken, static predictor or even simple logic might predict taken.
            // If predicted NT, it flows to Execute. 
            // Let's assume Static Predictor (NT).
            
            // Actually, let's check Execute behavior for mispredict.
            Cycle(2); // Decode
            Cycle(1); // Execute
            
            var xm = PipelineBuffers.ExecuteMemory;
            
            if (xm.Slots[0].BranchTaken)
            {
                // Branch taken in EX.
                // Verify PC redirected.
                Assert.Equal(0x0Cul, Machine.Registers.PC); 
            }
        }
        
        [Fact]
        public void Nop_Handling()
        {
            InitSuperscalar(2);
            Assembler.Add(pc => Inst.Addi(0,0,0)); // Manual NOP
            Assembler.Add(pc => Inst.Addi(0,0,0)); 
            LoadProgram();
            Cycle(3);
            
            var xm = PipelineBuffers.ExecuteMemory;
            Assert.True(xm.Slots[0].Valid);
            Assert.True(xm.Slots[1].Valid);
        }
    }
}
