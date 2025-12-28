using Xunit;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Components;

public class MemoryTests
{
    [Fact]
    public void LittleEndian_Byte_Packing()
    {
        var mem = new TestMemoryBus(1024);
        
        // Write 0xAABBCCDD (32-bit)
        // Memory: [DD] [CC] [BB] [AA]
        mem.WriteWord(0, 0xAABBCCDD);
        
        Assert.Equal(0xDD, mem.ReadByte(0));
        Assert.Equal(0xCC, mem.ReadByte(1));
        Assert.Equal(0xBB, mem.ReadByte(2));
        Assert.Equal(0xAA, mem.ReadByte(3));
    }

    [Fact]
    public void Double_Spans_Two_Words()
    {
        var mem = new TestMemoryBus(1024);
        ulong val = 0x1111222233334444;
        mem.WriteDouble(0, val);
        
        // Low word: 0x33334444
        Assert.Equal(0x33334444u, mem.ReadWord(0));
        // High word: 0x11112222
        Assert.Equal(0x11112222u, mem.ReadWord(4));
    }
}
