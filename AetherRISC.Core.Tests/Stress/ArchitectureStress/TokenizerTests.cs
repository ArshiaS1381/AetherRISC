using AetherRISC.Core.Helpers;
using Xunit;
using System.Linq;
using System.Reflection;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class TokenizerTests
{
    [Fact]
    public void Verify_Hex_Tokenization()
    {
        var assembler = new SourceAssembler("lui t0, 0x12345");
        var method = typeof(SourceAssembler).GetMethod("Tokenize", BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Fix CS8602: Check method is not null before invoking
        Assert.NotNull(method);

        // Fix CS8600: Invoke returns object?, cast safely
        var result = method!.Invoke(assembler, new object[] { "lui t0, 0x12345" });
        
        // Ensure result is not null before casting
        Assert.NotNull(result);
        var tokens = (string[])result!;

        // Fix CS8602: Dereference 'tokens' safely (though array access is usually safe if not null)
        Assert.Equal("0x12345", tokens[2]);
    }
}
