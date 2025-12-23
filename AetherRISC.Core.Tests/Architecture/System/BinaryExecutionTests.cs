using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.System;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.ISA.Decoding;
using System.IO;

namespace AetherRISC.Core.Tests.Architecture.System;

public class BinaryExecutionTests
{
    [Fact]
    public void LoadAndExecute_SimpleAdd_Binary()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024 * 1024); // 1MB Memory
        
        // Manual binary (Little Endian): 
        // 0: ADDI x1, x0, 10 -> 0x00A00093
        // 4: ADDI x2, x0, 20 -> 0x01400113
        // 8: ADD  x3, x1, x2 -> 0x002081B3
        // C: EBREAK          -> 0x00100073
        byte[] mockBin = { 
            0x93, 0x00, 0xA0, 0x00, 
            0x13, 0x01, 0x40, 0x01, 
            0xB3, 0x81, 0x20, 0x00, 
            0x73, 0x00, 0x10, 0x00 
        };
        
        string path = "test_simple.bin";
        File.WriteAllBytes(path, mockBin);

        // Configure the decoder with the Base I family
        var decoder = new InstructionDecoder();
        new AetherRISC.Core.Architecture.ISA.Families.Rv64iFamily().Register(decoder);

        // Load and Run
        BinaryLoader.Load(state, path, 0x0);
        var runner = new AetherRunner(state, decoder);
        runner.ExecuteUntilHalt();

        // Check result: x3 should be 30
        Assert.Equal(30u, state.Registers.Read(3));
        
        File.Delete(path);
    }
}
