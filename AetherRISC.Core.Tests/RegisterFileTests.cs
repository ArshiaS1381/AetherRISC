using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Registers;

namespace AetherRISC.Core.Tests;

public class RegisterFileTests
{
    private MachineState CreateTestMachine() => new MachineState(SystemConfig.Rv64());

    [Fact]
    public void Registers_Should_Read_And_Write_Correctly()
    {
        var machine = CreateTestMachine();
        machine.Registers.Write(1, 42);
        Assert.Equal((ulong)42, machine.Registers.Read(1));
    }

    [Fact]
    public void Register_Zero_Should_Always_Be_Zero()
    {
        var machine = CreateTestMachine();
        machine.Registers.Write(0, 9999);
        Assert.Equal((ulong)0, machine.Registers.Read(0));
    }

    [Fact]
    public void Should_Support_64Bit_Values()
    {
        var machine = CreateTestMachine();
        ulong bigValue = 0xDEAD_BEEF_CAFE_BABE; 
        machine.Registers.Write(10, bigValue);
        Assert.Equal(bigValue, machine.Registers.Read(10));
    }
}
