using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Integration.Diagnostics
{
    public class DeepSystemProbe : AetherRISC.Tests.Infrastructure.PipelineTestFixture
    {
        private readonly ITestOutputHelper _output;
        public DeepSystemProbe(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Probe_Ble_Pseudo_Logic()
        {
            var source = @"
                .text
                li x1, 5
                li x2, 3
                ble x1, x2, target 
                li x3, 1
                j end
                target:
                li x3, 0
                end:
                ebreak
            ";

            InitPipeline(1);
            var asm = new AetherRISC.Core.Assembler.SourceAssembler(source);
            asm.Assemble(Machine);
            Machine.Registers.PC = asm.TextBase;

            Cycle(20);

            if (Machine.Registers.Read(3) == 0)
                 _output.WriteLine("CRITICAL FAILURE: BLE logic is inverted!");
            Assert.Equal(1ul, Machine.Registers.Read(3));
        }

        [Fact]
        public void Probe_Data_Alignment()
        {
            var source = @"
                .data
                .byte 0xFF
                .align 2
                target: .word 0xDEADBEEF
                
                .text
                la x1, target
                lwu x2, 0(x1)
                ebreak
            ";

            InitPipeline(1);
            var asm = new AetherRISC.Core.Assembler.SourceAssembler(source);
            asm.Assemble(Machine);
            Machine.Registers.PC = asm.TextBase;

            Cycle(10);

            ulong addr = Machine.Registers.Read(1);
            ulong val = Machine.Registers.Read(2);

            _output.WriteLine($"Target Address: {addr:X}");
            _output.WriteLine($"Loaded Value:   {val:X}");

            Assert.True(addr % 4 == 0, $"Address {addr:X} is not 4-byte aligned.");
            Assert.Equal(0xDEADBEEFul, val);
        }

        [Fact]
        public void Probe_Load_Load_Branch_CycleTrace()
        {
            InitPipeline(1);
            Machine.Memory.WriteWord(0x100, 10);
            Machine.Memory.WriteWord(0x104, 20);
            Machine.Registers.Write(1, 0x100);

            Assembler.Add(pc => Inst.Lw(2, 1, 0));
            Assembler.Add(pc => Inst.Lw(3, 1, 4));
            Assembler.Add(pc => Inst.Beq(2, 3, 8));
            Assembler.Add(pc => Inst.Addi(4, 0, 1)); 
            Assembler.Add(pc => Inst.Ebreak(0, 0, 1));

            LoadProgram();

            Cycle(4); // HAZARD POINT
            
            var decBuf = Pipeline.PipelineState.DecodeExecute;
            var ifId = Pipeline.PipelineState.FetchDecode;

            // In Superscalar, 'IsEmpty' checks if ALL slots are empty/invalid
            Assert.True(decBuf.IsEmpty, "Pipeline failed to insert Bubble in Cycle 4");
            Assert.True(ifId.IsStalled, "Pipeline failed to Stall Fetch in Cycle 4");
        }

        [Fact]
        public void Probe_Store_Writeback()
        {
            InitPipeline(1);
            Machine.Registers.Write(1, 0x100);
            Machine.Registers.Write(2, 0xCAFE);

            Assembler.Add(pc => Inst.Sw(1, 2, 0)); 
            Assembler.Add(pc => Inst.Lw(3, 1, 0)); 
            
            LoadProgram();
            Cycle(10);

            Assert.Equal(0xCAFEu, Machine.Memory.ReadWord(0x100));
            Assert.Equal(0xCAFEul, Machine.Registers.Read(3));
        }
    }
}
