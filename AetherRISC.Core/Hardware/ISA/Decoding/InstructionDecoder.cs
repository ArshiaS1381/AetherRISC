using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Hardware.ISA.Base;

namespace AetherRISC.Core.Hardware.ISA.Decoding;

public class InstructionDecoder {
    public IInstruction Decode(uint inst) {
        if (inst == 0) return new NopInstruction();
        uint op = inst & 0x7F;
        int rd = (int)((inst >> 7) & 0x1F);
        int rs1 = (int)((inst >> 15) & 0x1F);
        int rs2 = (int)((inst >> 20) & 0x1F);

        switch (op) {
            case 0x13: return new AddiInstruction(rd, rs1, (int)inst >> 20);
            case 0x33: 
                // R-Type Decoding Logic
                int funct3 = (int)((inst >> 12) & 0x7);
                int funct7 = (int)((inst >> 25) & 0x7F);
                
                string m = "ADD"; // Default
                if (funct3 == 0) {
                    if (funct7 == 0x20) m = "SUB";      // 0100000
                    else if (funct7 == 0x01) m = "MUL"; // 0000001
                    else m = "ADD";                     // 0000000
                }
                else if (funct3 == 2) m = "SLT";
                
                return new AluInstruction(m, AluOp.Add, rd, rs1, rs2);
                
            case 0x03: return new LdInstruction(rd, rs1, (int)inst >> 20);
            case 0x23: // S-Type
                int sImm = ((int)((inst >> 25) & 0x7F) << 5) | (int)((inst >> 7) & 0x1F);
                if ((sImm & 0x800) != 0) sImm |= unchecked((int)0xFFFFF000);
                return new SdInstruction(rs1, rs2, sImm);
            case 0x63: // B-Type
                int bImm = ((int)(inst >> 31) << 12) | (((int)inst >> 7 & 0x1) << 11) | (((int)inst >> 25 & 0x3F) << 5) | (((int)inst >> 8 & 0xF) << 1);
                if ((bImm & 0x1000) != 0) bImm |= unchecked((int)0xFFFFE000);
                return new BneInstruction(rs1, rs2, bImm);
            case 0x6F: // J-Type
                int jImm = ((int)(inst >> 31) << 20) | (((int)inst >> 12 & 0xFF) << 12) | (((int)inst >> 20 & 0x1) << 11) | (((int)inst >> 21 & 0x3FF) << 1);
                if ((jImm & 0x100000) != 0) jImm |= unchecked((int)0xFFE00000);
                return new JalInstruction(rd, jImm);
            case 0x67: return new JalrInstruction(rd, rs1, (int)inst >> 20);
            case 0x73: return new EcallInstruction();
            default: return new NopInstruction();
        }
    }
}
