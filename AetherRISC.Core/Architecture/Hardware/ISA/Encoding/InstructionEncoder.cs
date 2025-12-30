using System;
using System.Collections.Generic;
using System.Reflection;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

public static partial class InstructionEncoder
{
    private static readonly Dictionary<string, Func<IInstruction, uint>> _encoders = new();

    private static void Register(string mnemonic, Func<IInstruction, uint> encoder) 
    {
        _encoders[mnemonic] = encoder;
    }

    static partial void RegisterGenerated();

    static InstructionEncoder()
    {
        RegisterGenerated();
    }

    public static uint Encode(IInstruction inst)
    {
        if (_encoders.TryGetValue(inst.Mnemonic, out var encoder))
        {
            return encoder(inst);
        }

        var type = inst.GetType();
        var attr = type.GetCustomAttribute<RiscvInstructionAttribute>();

        if (attr == null)
            throw new InvalidOperationException($"Instruction {type.Name} missing [RiscvInstruction] attribute.");

        uint opcode = attr.Opcode;
        uint rd = (uint)inst.Rd;
        uint rs1 = (uint)inst.Rs1;
        uint rs2 = (uint)inst.Rs2;
        uint funct3 = attr.Funct3;
        uint funct7 = attr.Funct7;
        int imm = inst.Imm;

        uint word = opcode & 0x7F;

        switch (attr.Type)
        {
            case RiscvEncodingType.R:
                word |= (rd & 0x1F) << 7;
                word |= (funct3 & 0x7) << 12;
                word |= (rs1 & 0x1F) << 15;
                word |= (rs2 & 0x1F) << 20;
                word |= (funct7 & 0x7F) << 25;
                break;
            case RiscvEncodingType.I:
                word |= (rd & 0x1F) << 7;
                word |= (funct3 & 0x7) << 12;
                word |= (rs1 & 0x1F) << 15;
                word |= ((uint)imm & 0xFFF) << 20;
                break;
            case RiscvEncodingType.S:
                word |= (((uint)imm & 0x1F) << 7);
                word |= (funct3 & 0x7) << 12;
                word |= (rs1 & 0x1F) << 15;
                word |= (rs2 & 0x1F) << 20;
                word |= ((((uint)imm >> 5) & 0x7F) << 25);
                break;
            case RiscvEncodingType.B:
                uint b12   = (uint)((imm >> 12) & 1);
                uint b11   = (uint)((imm >> 11) & 1);
                uint b10_5 = (uint)((imm >> 5) & 0x3F);
                uint b4_1  = (uint)((imm >> 1) & 0xF);

                word |= (b11 << 7);
                word |= (b4_1 << 8);
                word |= (funct3 & 0x7) << 12;
                word |= (rs1 & 0x1F) << 15;
                word |= (rs2 & 0x1F) << 20;
                word |= (b10_5 << 25);
                word |= (b12 << 31);
                break;
            case RiscvEncodingType.U:
                word |= (rd & 0x1F) << 7;
                word |= ((uint)imm & 0xFFFFF000);
                break;
            case RiscvEncodingType.J:
                uint j_imm20 = (uint)((imm >> 20) & 1);
                uint j_imm10_1 = (uint)((imm >> 1) & 0x3FF);
                uint j_imm11 = (uint)((imm >> 11) & 1);
                uint j_imm19_12 = (uint)((imm >> 12) & 0xFF);
                word |= (rd & 0x1F) << 7;
                word |= (j_imm19_12 << 12);
                word |= (j_imm11 << 20);
                word |= (j_imm10_1 << 21);
                word |= (j_imm20 << 31);
                break;
            case RiscvEncodingType.ShiftImm:
                word |= (rd & 0x1F) << 7;
                word |= (funct3 & 0x7) << 12;
                word |= (rs1 & 0x1F) << 15;
                word |= ((uint)imm & 0x3F) << 20;
                word |= (attr.Funct6 & 0x3F) << 26;
                break;
            case RiscvEncodingType.ZbbUnary:
                word |= (rd & 0x1F) << 7;
                word |= (funct3 & 0x7) << 12;
                word |= (rs1 & 0x1F) << 15;
                if (attr.Rs2Sel != 0) word |= ((uint)attr.Rs2Sel & 0x1F) << 20;
                word |= (funct7 & 0x7F) << 25;
                break;
        }
        return word;
    }

    public static uint GenR(uint op, uint f3, uint f7, IInstruction i) => 
        (op & 0x7F) | ((uint)(i.Rd & 0x1F) << 7) | ((f3 & 0x7) << 12) | ((uint)(i.Rs1 & 0x1F) << 15) | ((uint)(i.Rs2 & 0x1F) << 20) | ((f7 & 0x7F) << 25);
    
    public static uint GenI(uint op, uint f3, IInstruction i) => 
        (op & 0x7F) | ((uint)(i.Rd & 0x1F) << 7) | ((f3 & 0x7) << 12) | ((uint)(i.Rs1 & 0x1F) << 15) | ((uint)(i.Imm & 0xFFF) << 20);
    
    public static uint GenS(uint op, uint f3, IInstruction i) => 
        (op & 0x7F) | (((uint)i.Imm & 0x1F) << 7) | ((f3 & 0x7) << 12) | ((uint)(i.Rs1 & 0x1F) << 15) | ((uint)(i.Rs2 & 0x1F) << 20) | ((((uint)i.Imm >> 5) & 0x7F) << 25);
    
    public static uint GenB(uint op, uint f3, IInstruction i)
    {
        int imm = i.Imm;

        uint b12   = (uint)((imm >> 12) & 1);
        uint b11   = (uint)((imm >> 11) & 1);
        uint b10_5 = (uint)((imm >> 5) & 0x3F);
        uint b4_1  = (uint)((imm >> 1) & 0xF);

        return (op & 0x7F)
            | (b11 << 7)
            | (b4_1 << 8)
            | ((f3 & 0x7) << 12)
            | ((uint)(i.Rs1 & 0x1F) << 15)
            | ((uint)(i.Rs2 & 0x1F) << 20)
            | (b10_5 << 25)
            | (b12 << 31);
    }
    
    public static uint GenU(uint op, IInstruction i) => 
        (op & 0x7F) | ((uint)(i.Rd & 0x1F) << 7) | ((uint)i.Imm & 0xFFFFF000);
    
    public static uint GenJ(uint op, IInstruction i) 
    { 
        int imm = i.Imm;
        uint j20 = (uint)((imm >> 20) & 1);
        uint j10_1 = (uint)((imm >> 1) & 0x3FF);
        uint j11 = (uint)((imm >> 11) & 1);
        uint j19_12 = (uint)((imm >> 12) & 0xFF);
        return (op & 0x7F) 
            | ((uint)(i.Rd & 0x1F) << 7) 
            | (j19_12 << 12) 
            | (j11 << 20) 
            | (j10_1 << 21) 
            | (j20 << 31); 
    }
    
    public static uint GenShiftImm(uint op, uint f3, uint f6, IInstruction i) => 
        (op & 0x7F) | ((uint)(i.Rd & 0x1F) << 7) | ((f3 & 0x7) << 12) | ((uint)(i.Rs1 & 0x1F) << 15) | ((uint)(i.Imm & 0x3F) << 20) | ((f6 & 0x3F) << 26);
    
    public static uint GenZbbUnary(uint op, uint f3, uint f7, uint rs2Sel, IInstruction i) => 
        (op & 0x7F) | ((uint)(i.Rd & 0x1F) << 7) | ((f3 & 0x7) << 12) | ((uint)(i.Rs1 & 0x1F) << 15) | ((rs2Sel & 0x1F) << 20) | ((f7 & 0x7F) << 25);
}
