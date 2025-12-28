using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Simulation;

namespace AetherRISC.Tests.Integration.OS;

public class SystemCallTests : CpuTestFixture
{
    [Fact]
    public void Linux_Write_Standard_Flow()
    {
        Init64();
        Machine.Host = new MultiOSHandler { Kind = OSKind.Linux, Silent = true };

        string code = @"
            .data
            msg: .asciz ""Hi""
            .text
            li a0, 1        # stdout
            la a1, msg      # buffer
            li a2, 2        # len
            li a7, 64       # sys_write
            ecall
        ";

        var asm = new SourceAssembler(code);
        asm.Assemble(Machine);
        
        Runner.Run(50);

        // Linux write returns number of bytes written in a0
        AssertReg(10, 2ul); 
    }

    [Fact]
    public void Rars_Random_Range()
    {
        Init64();
        Machine.Host = new MultiOSHandler { Kind = OSKind.RARS, Silent = true };

        string code = @"
            li a1, 100      # Max
            li a7, 42       # RandIntRange
            ecall
        ";

        var asm = new SourceAssembler(code);
        asm.Assemble(Machine);
        
        Runner.Run(10);

        // Result in a0 should be < 100
        ulong res = Machine.Registers.Read(10);
        Assert.True(res < 100);
    }
}
