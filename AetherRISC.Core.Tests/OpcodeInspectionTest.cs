using Xunit;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests;

public class OpcodeInspectionTest
{
    [Fact]
    public void ADD_Should_Not_Be_SUB()
    {
        // Target: ADD x10, x10, x10
        // Correct Binary: 0000000 01010 01010 000 01010 0110011
        // Correct Hex:    0x00A50533
        
        // SUB Binary:     0100000 ...
        // SUB Hex:        0x40A50533
        
        var addInst = Inst.Add(10, 10, 10);
        uint encoded = InstructionEncoder.Encode(addInst);

        // If this fails with Actual: 40A50533, your Encoder is turning ADDs into SUBs
        Assert.Equal((uint)0x00A50533, encoded);
    }
}
