using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbcDiagnosticTests
{
    [Fact]
    public void Verify_Carryless_Logic()
    {
        // Simple case: 2 (10b) clmul 2 (10b) = 4 (100b)
        var (lo, hi) = CarrylessMath.Clmul128(2, 2);
        Assert.Equal(4ul, lo);
        Assert.Equal(0ul, hi);

        // XOR identity: 3 (11b) clmul 3 (11b)
        // (11 << 1) ^ (11 << 0) = 110 ^ 011 = 101 (5)
        (lo, hi) = CarrylessMath.Clmul128(3, 3);
        Assert.Equal(5ul, lo);
    }
}
