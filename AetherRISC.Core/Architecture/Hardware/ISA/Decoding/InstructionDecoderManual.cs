using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;
using AetherRISC.Core.Architecture.Hardware.ISA.Utils;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

public partial class InstructionDecoder
{
    private IInstruction? DecodeManual(uint raw)
    {
        uint opcode = raw & 0x7F;
        int f3 = (int)((raw >> 12) & 0x7);
        int f7 = (int)((raw >> 25) & 0x7F);

        int rd = (int)((raw >> 7) & 0x1F);
        int rs1 = (int)((raw >> 15) & 0x1F);
        int rs2 = (int)((raw >> 20) & 0x1F);

        // OP-IMM (0x13)
        if (opcode == 0x13)
        {
            int iImm = BitUtils.ExtractITypeImm(raw);
            if (f3 == 0) return new AddiInstruction(rd, rs1, iImm);
            if (f3 == 2) return new SltiInstruction(rd, rs1, iImm);
            if (f3 == 3) return new SltiuInstruction(rd, rs1, iImm);
            if (f3 == 4) return new XoriInstruction(rd, rs1, iImm);
            if (f3 == 6) return new OriInstruction(rd, rs1, iImm);
            if (f3 == 7) return new AndiInstruction(rd, rs1, iImm);
            if (f3 == 1) return new SlliInstruction(rd, rs1, iImm); // Shamt handled inside
            if (f3 == 5) {
                 if (((raw >> 26) & 0x3F) == 0) return new SrliInstruction(rd, rs1, iImm);
                 if (((raw >> 26) & 0x3F) == 0x10) return new SraiInstruction(rd, rs1, iImm);
            }
        }
        
        // OP (0x33)
        if (opcode == 0x33)
        {
             if (f7 == 0) {
                 if (f3 == 0) return new AddInstruction(rd, rs1, rs2);
                 if (f3 == 1) return new SllInstruction(rd, rs1, rs2);
                 if (f3 == 2) return new SltInstruction(rd, rs1, rs2);
                 if (f3 == 3) return new SltuInstruction(rd, rs1, rs2);
                 if (f3 == 4) return new XorInstruction(rd, rs1, rs2);
                 if (f3 == 5) return new SrlInstruction(rd, rs1, rs2);
                 if (f3 == 6) return new OrInstruction(rd, rs1, rs2);
                 if (f3 == 7) return new AndInstruction(rd, rs1, rs2);
             }
             if (f7 == 0x20) {
                 if (f3 == 0) return new SubInstruction(rd, rs1, rs2);
                 if (f3 == 5) return new SraInstruction(rd, rs1, rs2);
             }
        }

        // LOAD (0x03)
        if (opcode == 0x03)
        {
            int imm = BitUtils.ExtractITypeImm(raw);
            if (f3 == 0) return new LbInstruction(rd, rs1, imm);
            if (f3 == 1) return new LhInstruction(rd, rs1, imm);
            if (f3 == 2) return new LwInstruction(rd, rs1, imm);
            if (f3 == 3) return new LdInstruction(rd, rs1, imm);
            if (f3 == 4) return new LbuInstruction(rd, rs1, imm);
            if (f3 == 5) return new LhuInstruction(rd, rs1, imm);
            if (f3 == 6) return new LwuInstruction(rd, rs1, imm);
        }

        // STORE (0x23)
        if (opcode == 0x23)
        {
             int imm = BitUtils.ExtractSTypeImm(raw);
             if (f3 == 0) return new SbInstruction(rs1, rs2, imm);
             if (f3 == 1) return new ShInstruction(rs1, rs2, imm);
             if (f3 == 2) return new SwInstruction(rs1, rs2, imm);
             if (f3 == 3) return new SdInstruction(rs1, rs2, imm);
        }

        // BRANCH (0x63)
        if (opcode == 0x63)
        {
            int imm = BitUtils.ExtractBTypeImm(raw);
            if (f3 == 0) return new BeqInstruction(rs1, rs2, imm);
            if (f3 == 1) return new BneInstruction(rs1, rs2, imm);
            if (f3 == 4) return new BltInstruction(rs1, rs2, imm);
            if (f3 == 5) return new BgeInstruction(rs1, rs2, imm);
            if (f3 == 6) return new BltuInstruction(rs1, rs2, imm);
            if (f3 == 7) return new BgeuInstruction(rs1, rs2, imm);
        }

        // LUI / AUIPC
        if (opcode == 0x37) return new LuiInstruction(rd, BitUtils.ExtractUTypeImm(raw));
        if (opcode == 0x17) return new AuipcInstruction(rd, BitUtils.ExtractUTypeImm(raw));

        // JAL / JALR
        if (opcode == 0x6F) return new JalInstruction(rd, BitUtils.ExtractJTypeImm(raw));
        if (opcode == 0x67) return new JalrInstruction(rd, rs1, BitUtils.ExtractITypeImm(raw));
        
        // SYSTEM / ZICSR (0x73)
        if (opcode == 0x73)
        {
            int imm = BitUtils.ExtractITypeImm(raw);
            if (f3 == 0 && f7 == 0) {
                 if (imm == 0) return new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem.EcallInstruction(rd, rs1, imm);
                 if (imm == 1) return new AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem.EbreakInstruction(rd, rs1, imm);
            }
            if (f3 == 1) return new CsrrwInstruction(rd, rs1, imm);
            if (f3 == 2) return new CsrrsInstruction(rd, rs1, imm);
            if (f3 == 3) return new CsrrcInstruction(rd, rs1, imm);
            if (f3 == 5) return new CsrrwiInstruction(rd, rs1, imm);
            if (f3 == 6) return new CsrrsiInstruction(rd, rs1, imm);
            if (f3 == 7) return new CsrrciInstruction(rd, rs1, imm);
        }

        return null;
    }
}
