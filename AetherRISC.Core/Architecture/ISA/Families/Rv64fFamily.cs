using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Architecture.ISA.Extensions.F;

namespace AetherRISC.Core.Architecture.ISA.Families;

public class Rv64fFamily : IInstructionFamily
{
    public void Register(InstructionDecoder decoder)
    {
        // Encoders
        InstructionEncoder.Register("FLW", i => InstructionEncoder.GenI(0x07, 2, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("FSW", i => InstructionEncoder.GenS(0x27, 2, (uint)i.Rs1, (uint)i.Rs2, (uint)i.Imm));

        // Decoder: LOAD-FP (0x07)
        decoder.RegisterOpcode(0x07, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int imm = (int)((inst >> 20));
            if ((imm & 0x800) != 0) imm |= unchecked((int)0xFFFFF000); // Sign extend

            return f3 == 2 ? new FlwInstruction(rd, rs1, imm) : new NopInstruction(); 
        });

        // Decoder: STORE-FP (0x27)
        decoder.RegisterOpcode(0x27, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int rs2 = (int)((inst >> 20) & 0x1F);
            int immlo = (int)((inst >> 7) & 0x1F);
            int immhi = (int)((inst >> 25) & 0x7F);
            int imm = (immhi << 5) | immlo;
            if ((imm & 0x800) != 0) imm |= unchecked((int)0xFFFFF000);

            return f3 == 2 ? new FswInstruction(rs1, rs2, imm) : new NopInstruction();
        });
    }
}
