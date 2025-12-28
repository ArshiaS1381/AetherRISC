using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class CompressedInstructionTests
{
    private static void WriteHalfWord(SystemBus bus, uint addr, ushort v)
    {
        bus.WriteByte(addr, (byte)(v & 0xFF));
        bus.WriteByte(addr + 1, (byte)((v >> 8) & 0xFF));
    }

    // Helpers to build a few C encodings (RV64C subset)
    // C.ADDI (quadrant 1, funct3=000): imm[5]=bit12, imm[4:0]=bits6:2, rd=bits11:7, op=01
    private static ushort CAddi(int rd, int imm)
    {
        int uimm = imm & 0x3F;
        int imm5 = (uimm >> 5) & 1;
        int imm4_0 = uimm & 0x1F;

        return (ushort)(
            (0b000 << 13)
            | (imm5 << 12)
            | ((rd & 0x1F) << 7)
            | (imm4_0 << 2)
            | 0b01
        );
    }

    // C.LI (quadrant 1, funct3=010)
    private static ushort CLi(int rd, int imm)
    {
        int uimm = imm & 0x3F;
        int imm5 = (uimm >> 5) & 1;
        int imm4_0 = uimm & 0x1F;

        return (ushort)(
            (0b010 << 13)
            | (imm5 << 12)
            | ((rd & 0x1F) << 7)
            | (imm4_0 << 2)
            | 0b01
        );
    }

    // C.MV / C.ADD (quadrant 2, funct3=100): rd=bits11:7, rs2=bits6:2, bit12=0 mv / 1 add
    private static ushort CMv(int rd, int rs2)
    {
        return (ushort)(
            (0b100 << 13) | (0 << 12) | ((rd & 0x1F) << 7) | ((rs2 & 0x1F) << 2) | 0b10
        );
    }

    private static ushort CAdd(int rd, int rs2)
    {
        return (ushort)(
            (0b100 << 13) | (1 << 12) | ((rd & 0x1F) << 7) | ((rs2 & 0x1F) << 2) | 0b10
        );
    }

    private const ushort CEbreak = 0x9002;

    [Fact]
    public void C_Addi_And_C_Ebreak_Execute()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x2000);
        s.Host = new MultiOSHandler { Silent = true };
        s.ProgramCounter = 0;

        // c.addi x5, 7 ; c.ebreak
        WriteHalfWord((SystemBus)s.Memory, 0, CAddi(5, 7));
        WriteHalfWord((SystemBus)s.Memory, 2, CEbreak);

        new PipelinedRunner(s, new NullLogger()).Run(30);

        Assert.Equal(7ul, s.Registers.Read(5));
    }

    [Fact]
    public void C_Li_Mv_Add_Chain_Works()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x2000);
        s.Host = new MultiOSHandler { Silent = true };
        s.ProgramCounter = 0;

        // c.li x6, 9
        // c.mv x7, x6
        // c.add x7, x6
        // c.ebreak
        WriteHalfWord((SystemBus)s.Memory, 0, CLi(6, 9));
        WriteHalfWord((SystemBus)s.Memory, 2, CMv(7, 6));
        WriteHalfWord((SystemBus)s.Memory, 4, CAdd(7, 6));
        WriteHalfWord((SystemBus)s.Memory, 6, CEbreak);

        new PipelinedRunner(s, new NullLogger()).Run(50);

        Assert.Equal(9ul, s.Registers.Read(6));
        Assert.Equal(18ul, s.Registers.Read(7));
    }

    [Fact]
    public void Mixed_Compressed_And_Normal_Instruction_Fetch_Works()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x2000);
        s.Host = new MultiOSHandler { Silent = true };
        s.ProgramCounter = 0;

        // c.addi x5, 1 (2 bytes)
        WriteHalfWord((SystemBus)s.Memory, 0, CAddi(5, 1));

        // normal 32-bit addi x5, x5, 2 at address 2
        // encoding: imm=2, rs1=5, rd=5, opcode=0x13
        uint addi = (2u << 20) | (5u << 15) | (0u << 12) | (5u << 7) | 0x13u;
        s.Memory.WriteWord(2, addi);

        // c.ebreak at address 6
        WriteHalfWord((SystemBus)s.Memory, 6, CEbreak);

        new PipelinedRunner(s, new NullLogger()).Run(60);

        Assert.Equal(3ul, s.Registers.Read(5));
    }
}
