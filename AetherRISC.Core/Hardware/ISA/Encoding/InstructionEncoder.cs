using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Hardware.ISA.Base;

namespace AetherRISC.Core.Hardware.ISA.Encoding;

public static class InstructionEncoder
{
    public static uint Encode(IInstruction inst)
    {
        switch (inst)
        {
            case AluInstruction alu: return EncodeRType(alu);
            case AddiInstruction addi: return EncodeIType(0x13, 0x0, addi.Rd, addi.Rs1, addi.Imm);
            case BneInstruction bne:   return EncodeBType(0x63, 0x1, bne.Rs1, bne.Rs2, bne.Imm);
            case EcallInstruction:     return 0x00000073; 
            case NopInstruction:       return 0x00000013; 
            case LuiInstruction lui:   return EncodeUType(0x37, lui.Rd, lui.Imm);
            case JalInstruction jal:   return EncodeJType(0x6F, jal.Rd, jal.Imm);
            case JalrInstruction jalr: return EncodeIType(0x67, 0x0, jalr.Rd, jalr.Rs1, jalr.Imm);

            // --- 32/64 Ops ---
            case AddwInstruction addw:  return EncodeRTypeRaw(0x3B, 0x0, 0x0, addw.Rd, addw.Rs1, addw.Rs2);
            case AddiwInstruction adiw: return EncodeIType(0x1B, 0x0, adiw.Rd, adiw.Rs1, adiw.Imm);
            case LwInstruction lw:     return EncodeIType(0x03, 0x2, lw.Rd, lw.Rs1, lw.Imm);
            case LdInstruction ld:     return EncodeIType(0x03, 0x3, ld.Rd, ld.Rs1, ld.Imm); 
            case SwInstruction sw:     return EncodeSType(0x23, 0x2, sw.Rs1, sw.Rs2, sw.Imm);
            case SdInstruction sd:     return EncodeSType(0x23, 0x3, sd.Rs1, sd.Rs2, sd.Imm);
            
            default: return 0x00000013; 
        }
    }

    private static uint EncodeRTypeRaw(uint opcode, uint funct3, uint funct7, int rd, int rs1, int rs2)
    {
        return (funct7 << 25) | ((uint)rs2 << 20) | ((uint)rs1 << 15) | (funct3 << 12) | ((uint)rd << 7) | opcode;
    }

    private static uint EncodeRType(AluInstruction inst)
    {
        uint funct3 = 0, funct7 = 0;
        switch (inst.Mnemonic) {
            case "ADD": funct3=0; funct7=0; break;
            case "SUB": funct3=0; funct7=0x20; break;
            case "MUL": funct3=0; funct7=0x01; break; // M Extension
            case "SLT": funct3=2; funct7=0; break; 
        }
        return EncodeRTypeRaw(0x33, funct3, funct7, inst.Rd, inst.Rs1, inst.Rs2);
    }

    private static uint EncodeIType(uint opcode, uint funct3, int rd, int rs1, int imm)
    {
        uint imm12 = (uint)imm & 0xFFF;
        return (imm12 << 20) | ((uint)rs1 << 15) | (funct3 << 12) | ((uint)rd << 7) | opcode;
    }

    private static uint EncodeSType(uint opcode, uint funct3, int rs1, int rs2, int imm)
    {
        uint imm11_5 = ((uint)imm >> 5) & 0x7F;
        uint imm4_0  = (uint)imm & 0x1F;
        return (imm11_5 << 25) | ((uint)rs2 << 20) | ((uint)rs1 << 15) | (funct3 << 12) | (imm4_0 << 7) | opcode;
    }

    private static uint EncodeBType(uint opcode, uint funct3, int rs1, int rs2, int imm)
    {
        uint bit12 = ((uint)imm >> 12) & 0x1;
        uint bit10_5 = ((uint)imm >> 5) & 0x3F;
        uint bit4_1 = ((uint)imm >> 1) & 0xF;
        uint bit11 = ((uint)imm >> 11) & 0x1;
        return (bit12 << 31) | (bit10_5 << 25) | ((uint)rs2 << 20) | ((uint)rs1 << 15) | (funct3 << 12) | (bit4_1 << 8) | (bit11 << 7) | opcode;
    }
    
    private static uint EncodeUType(uint opcode, int rd, int imm) => ((uint)imm & 0xFFFFF000) | ((uint)rd << 7) | opcode;

    private static uint EncodeJType(uint opcode, int rd, int imm)
    {
        uint bit20 = ((uint)imm >> 20) & 0x1;
        uint bit10_1 = ((uint)imm >> 1) & 0x3FF;
        uint bit11 = ((uint)imm >> 11) & 0x1;
        uint bit19_12 = ((uint)imm >> 12) & 0xFF;
        return (bit20 << 31) | (bit10_1 << 21) | (bit11 << 20) | (bit19_12 << 12) | ((uint)rd << 7) | opcode;
    }
}
