using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Architecture.ISA.Extensions.A;

namespace AetherRISC.Core.Architecture.ISA.Families;

public class Rv64aFamily : IInstructionFamily
{
    public void Register(InstructionDecoder decoder)
    {
        // Encoders (funct5 maps to specific AMO operations)
        // 00001 = SWAP
        // 00000 = ADD
        // 00100 = XOR
        // 01100 = AND
        // 01000 = OR
        // 10000 = MIN
        // 10100 = MAX
        // 11000 = MINU
        // 11100 = MAXU

        // Helper to register both W (width=2) and D (width=3)
        void RegAmo(string name, int f5) {
            InstructionEncoder.Register($"{name}.W", i => InstructionEncoder.GenR(0x2F, 2, (uint)f5, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
            InstructionEncoder.Register($"{name}.D", i => InstructionEncoder.GenR(0x2F, 3, (uint)f5, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        }

        InstructionEncoder.Register("LR.W", i => InstructionEncoder.GenR(0x2F, 2, 0x02, (uint)i.Rd, (uint)i.Rs1, 0)); 
        InstructionEncoder.Register("SC.W", i => InstructionEncoder.GenR(0x2F, 2, 0x03, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("LR.D", i => InstructionEncoder.GenR(0x2F, 3, 0x02, (uint)i.Rd, (uint)i.Rs1, 0)); 
        InstructionEncoder.Register("SC.D", i => InstructionEncoder.GenR(0x2F, 3, 0x03, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));

        RegAmo("AMOSWAP", 0x01);
        RegAmo("AMOADD",  0x00);
        RegAmo("AMOXOR",  0x04);
        RegAmo("AMOAND",  0x0C);
        RegAmo("AMOOR",   0x08);
        RegAmo("AMOMIN",  0x10);
        RegAmo("AMOMAX",  0x14);
        RegAmo("AMOMINU", 0x18);
        RegAmo("AMOMAXU", 0x1C);

        // Decoder
        decoder.RegisterOpcode(0x2F, (inst) => {
            int width = (int)((inst >> 12) & 0x7); // 2=W, 3=D
            int f5 = (int)((inst >> 27) & 0x1F);
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int rs2 = (int)((inst >> 20) & 0x1F);

            bool isWord = (width == 2);
            if (width != 2 && width != 3) return new NopInstruction(); 

            return f5 switch {
                0x02 => new LrInstruction(rd, rs1, isWord),
                0x03 => new ScInstruction(rd, rs1, rs2, isWord),
                0x01 => new AmoSwapInstruction(rd, rs1, rs2, isWord),
                0x00 => new AmoAddInstruction(rd, rs1, rs2, isWord),
                0x04 => new AmoXorInstruction(rd, rs1, rs2, isWord),
                0x0C => new AmoAndInstruction(rd, rs1, rs2, isWord),
                0x08 => new AmoOrInstruction(rd, rs1, rs2, isWord),
                0x10 => new AmoMinInstruction(rd, rs1, rs2, isWord),
                0x14 => new AmoMaxInstruction(rd, rs1, rs2, isWord),
                0x18 => new AmoMinuInstruction(rd, rs1, rs2, isWord),
                0x1C => new AmoMaxuInstruction(rd, rs1, rs2, isWord),
                _ => new NopInstruction()
            };
        });
    }
}
