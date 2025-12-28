using Xunit;
using System;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Families;

namespace AetherRISC.Core.Tests.System;

public class DecoderDebuggingTests
{
    [Fact]
    public void Diagnostic_Identify_Opcode_Hijacking()
    {
        var decoder = new InstructionDecoder();
        
        // 1. Raw EBREAK binary
        uint ebreakRaw = 0x00100073; 

        // 2. Decode it
        var inst = decoder.Decode(ebreakRaw);

        // 3. Inspect the Result
        Console.WriteLine($"[Diagnostic] Input Hex: 0x{ebreakRaw:X}");
        Console.WriteLine($"[Diagnostic] Decoded Type: {inst.GetType().FullName}");
        Console.WriteLine($"[Diagnostic] Mnemonic: {inst.Mnemonic}");

        // 4. Identity Check
        if (inst is EcallInstruction)
        {
            throw new Exception("CRITICAL: EBREAK (0x00100073) was decoded as ECALL! This confirms an extension (likely Zicsr) is overwriting the 0x73 handler and defaulting to ECALL.");
        }

        Assert.IsType<EbreakInstruction>(inst);
    }

    [Fact]
    public void Diagnostic_Check_Registration_Order()
    {
        // This test manually builds a decoder to see if swapping order fixes it.
        // If this passes, we know Zicsr is the culprit.
        
        var decoder = new InstructionDecoder(); 
        // Note: The standard constructor loads Rv64i, then Zicsr.
        
        var inst = decoder.Decode(0x00100073);
        Assert.True(inst is EbreakInstruction, $"Standard Loading failed. Got: {inst.GetType().Name}");
    }
}


