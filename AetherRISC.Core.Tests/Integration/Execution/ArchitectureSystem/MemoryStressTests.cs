using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class MemoryStressTests
{
    private SystemBus _bus = new SystemBus(1024);

    [Fact]
    public void Byte_Writes_Should_Pack_Correctly_Into_Word()
    {
        // Explicitly use uint literals (0xAABBCCDDu) to prevent compiler confusion
        _bus.WriteWord(0x00u, 0xAABBCCDDu);
        
        uint val = _bus.ReadWord(0x00u);
        Assert.Equal(0xAABBCCDDu, val);
        
        _bus.WriteWord(0x04u, 0x11223344u);
        Assert.Equal(0x11223344u, _bus.ReadWord(0x04u));
    }

    [Fact]
    public void DoubleWord_Should_Span_Two_Words()
    {
        ulong bigVal = 0x1111222233334444;
        _bus.WriteDouble(0x10u, bigVal);

        // Lower address = Lower 32 bits (Little Endian)
        uint lower = _bus.ReadWord(0x10u);
        uint upper = _bus.ReadWord(0x14u);

        Assert.Equal(0x33334444u, lower);
        Assert.Equal(0x11112222u, upper);
    }

    [Fact]
    public void Memory_Should_Preserve_Data_Across_Ranges()
    {
        for (uint i = 0; i < 100; i += 4)
        {
            _bus.WriteWord(i, i * 1000);
        }

        for (uint i = 0; i < 100; i += 4)
        {
            Assert.Equal(i * 1000, _bus.ReadWord(i));
        }
    }
}



