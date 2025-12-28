using Xunit;
using AetherRISC.Core.Helpers;
using System.Reflection;

namespace AetherRISC.Tests.Unit.Components;

public class AssemblerInternalTests
{
    [Fact]
    public void Tokenizer_Splits_Registers_And_Hex()
    {
        var assembler = new SourceAssembler("");
        var method = typeof(SourceAssembler).GetMethod("Tokenize", BindingFlags.NonPublic | BindingFlags.Instance);
        
        Assert.NotNull(method);

        string line = "sw t0, 0x10(sp)";
        var result = method!.Invoke(assembler, new object[] { line }) as string[];

        Assert.NotNull(result);
        Assert.Equal("sw", result![0]);
        Assert.Equal("t0", result[1]);
        Assert.Equal("0x10", result[2]);
        Assert.Equal("sp", result[3]);
    }
}
