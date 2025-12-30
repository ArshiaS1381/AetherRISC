using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using System.IO;

namespace AetherRISC.Tests.Integration.Pipeline
{
    public class PipelineDiagnosticProbe
    {
        private readonly ITestOutputHelper _output;

        public PipelineDiagnosticProbe(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Probe_Bimodal_Loop_Behavior()
        {
            var asm = @"
                .text
                li t0, 3
            loop:
                addi t0, t0, -1
                bnez t0, loop
                ebreak
            ";
            RunProbe(asm, "bimodal", 40);
        }

        private void RunProbe(string asm, string predictor, int maxCycles)
        {
            _output.WriteLine($"=== PROBE START: {predictor.ToUpper()} ===");
            
            var config = SystemConfig.Rv64();
            var state = new MachineState(config);
            state.Memory = new SystemBus(1024 * 1024);
            state.Host = new MultiOSHandler { Output = new StringWriter(), Silent = true };

            var assembler = new SourceAssembler(asm) { TextBase = 0x80000000 };
            assembler.Assemble(state);
            state.ProgramCounter = 0x80000000;
            state.Registers.PC = 0x80000000;

            var cpu = new PipelineController(state, predictor, new ArchitectureSettings());
            
            _output.WriteLine(string.Format("{0,-6} | {1,-10} | {2,-10} | {3,-10} | {4,-15}", 
                "CYCLE", "FETCH PC", "PRED TAKEN", "EXEC PC", "ACTION"));
            _output.WriteLine(new string('-', 75));

            for (int i = 0; i < maxCycles; i++)
            {
                if (state.Halted) break;

                ulong fetchPC = state.Registers.PC;
                cpu.Cycle();

                // Just inspect Slot 0 for the scalar probe
                var fd = cpu.Buffers.FetchDecode.Slots[0];
                var em = cpu.Buffers.ExecuteMemory.Slots[0];

                string action = "";
                if (em.Misprediction) action = "MISPREDICT FLUSH";
                else if (state.Halted) action = "HALT";
                else if (em.Valid && em.BranchTaken) action = "BRANCH TAKEN";
                else if (em.Valid && em.DecodedInst != null) action = em.DecodedInst.GetType().Name.Replace("Instruction", "").ToUpper();
                else action = "BUBBLE";

                _output.WriteLine(string.Format("{0,-6} | {1,-10:X} | {2,-10} | {3,-10:X} | {4,-15}", 
                    i, fetchPC, fd.PredictedTaken ? "YES" : "NO", em.PC, action));
            }
            _output.WriteLine("=== PROBE END ===\n");
        }
    }
}
