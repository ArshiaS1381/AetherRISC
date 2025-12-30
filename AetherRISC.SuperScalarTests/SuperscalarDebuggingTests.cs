using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

namespace AetherRISC.SuperScalarTests
{
    public class SuperscalarDebuggingTests
    {
        [Fact]
        public void ManualDecoder_Decodes_ADDI_Correctly()
        {
            // Verify InstructionDecoderManual.cs logic directly
            // ADDI x1, x0, 10
            // Op: 0x13, F3: 0, Rd: 1, Rs1: 0, Imm: 10
            uint raw = 0x00A00093; 
            
            var decoder = new InstructionDecoder();
            var inst = decoder.Decode(raw);

            Assert.NotNull(inst);
            Assert.Equal("ADDI", inst.Mnemonic);
            Assert.Equal(1, inst.Rd);
            Assert.Equal(10, inst.Imm);
        }

        [Fact]
        public void ManualDecoder_Decodes_LW_Correctly()
        {
            // LW x2, 0(x1)
            // Op: 0x03, F3: 2, Rd: 2, Rs1: 1, Imm: 0
            uint raw = 0x0000A103;

            var decoder = new InstructionDecoder();
            var inst = decoder.Decode(raw);

            Assert.NotNull(inst);
            Assert.Equal("LW", inst.Mnemonic);
            Assert.Equal(2, inst.Rd);
            Assert.Equal(1, inst.Rs1);
        }

        [Fact]
        public void DataHazard_Detection_IsRobust()
        {
            // This tests the logic inside DataHazardUnit without running a full CPU.
            var unit = new DataHazardUnit();

            // 1. Setup Execute Stage with a LOAD to x5
            var exSlot = buffers.ExecuteMemory.Slots[0];
            exSlot.Valid = true;
            exSlot.MemRead = true; // Crucial for Load-Use
            exSlot.Rd = 5;
            exSlot.RegWrite = true;

            // 2. Setup Decode Stage with an ADDI using x5
            var idSlot = buffers.DecodeExecute.Slots[0];
            idSlot.Valid = true;
            idSlot.DecodedInst = new AddiInstruction(1, 5, 10); // Uses x5
            idSlot.RawInstruction = 0; // Dummy
            idSlot.PC = 4;

            // 3. Run Resolve
            unit.Resolve(buffers);

            // 4. Assert Stall happened
            Assert.True(buffers.FetchDecode.IsStalled, "Pipeline should stall Fetch on Load-Use hazard");
            Assert.True(buffers.DecodeExecute.IsStalled, "Pipeline should stall Decode on Load-Use hazard");
        }
    }
}
