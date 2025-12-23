using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Architecture.ISA.Extensions.M;

namespace AetherRISC.Core.Architecture.ISA.Families;

public class Rv64mFamily : IInstructionFamily
{
    public void Register(InstructionDecoder decoder)
    {
        // Encoders (Standard R-Type mapping)
        InstructionEncoder.Register("MUL",    i => InstructionEncoder.GenR(0x33, 0, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("MULH",   i => InstructionEncoder.GenR(0x33, 1, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("MULHSU", i => InstructionEncoder.GenR(0x33, 2, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("MULHU",  i => InstructionEncoder.GenR(0x33, 3, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("DIV",    i => InstructionEncoder.GenR(0x33, 4, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("DIVU",   i => InstructionEncoder.GenR(0x33, 5, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("REM",    i => InstructionEncoder.GenR(0x33, 6, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("REMU",   i => InstructionEncoder.GenR(0x33, 7, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        
        // 32-bit Word Ops (Opcode 0x3B, f7=1)
        InstructionEncoder.Register("MULW",   i => InstructionEncoder.GenR(0x3B, 0, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("DIVW",   i => InstructionEncoder.GenR(0x3B, 4, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("DIVUW",  i => InstructionEncoder.GenR(0x3B, 5, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("REMW",   i => InstructionEncoder.GenR(0x3B, 6, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("REMUW",  i => InstructionEncoder.GenR(0x3B, 7, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));

        // Decoder (0x33, f7=1 -> Standard 64-bit Math)
        decoder.RegisterOpcode(0x33, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int f7 = (int)((inst >> 25) & 0x7F);
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int rs2 = (int)((inst >> 20) & 0x1F);

            if (f7 != 1) return new NopInstruction(); 

            return f3 switch {
                0 => new MulInstruction(rd, rs1, rs2),
                1 => new MulhInstruction(rd, rs1, rs2),
                2 => new MulhsuInstruction(rd, rs1, rs2),
                3 => new MulhuInstruction(rd, rs1, rs2),
                4 => new DivInstruction(rd, rs1, rs2),
                5 => new DivuInstruction(rd, rs1, rs2),
                6 => new RemInstruction(rd, rs1, rs2),
                7 => new RemuInstruction(rd, rs1, rs2),
                _ => new NopInstruction()
            };
        });

        // Decoder (0x3B, f7=1 -> 32-bit Word Math)
        decoder.RegisterOpcode(0x3B, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int f7 = (int)((inst >> 25) & 0x7F);
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int rs2 = (int)((inst >> 20) & 0x1F);

            if (f7 != 1) return new NopInstruction(); 

            return f3 switch {
                0 => new MulwInstruction(rd, rs1, rs2),
                4 => new DivwInstruction(rd, rs1, rs2),
                5 => new DivuwInstruction(rd, rs1, rs2),
                6 => new RemwInstruction(rd, rs1, rs2),
                7 => new RemuwInstruction(rd, rs1, rs2),
                _ => new NopInstruction()
            };
        });
    }
}
