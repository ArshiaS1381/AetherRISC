using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Architecture.ISA.Extensions.Zicsr; // Import the separate files

namespace AetherRISC.Core.Architecture.ISA.Families;

public class Rv64ZicsrFamily : IInstructionFamily
{
    public void Register(InstructionDecoder decoder)
    {
        // Encoders
        InstructionEncoder.Register("CSRRW", i => InstructionEncoder.GenI(0x73, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("CSRRS", i => InstructionEncoder.GenI(0x73, 2, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("CSRRC", i => InstructionEncoder.GenI(0x73, 3, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("CSRRWI", i => InstructionEncoder.GenI(0x73, 5, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));

        // Decoder (0x73)
        decoder.RegisterOpcode(0x73, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int csr = (int)((inst >> 20) & 0xFFF); 

            return f3 switch {
                0 => (csr == 0 || csr == 1) ? new EcallInstruction() : new NopInstruction(),
                1 => new CsrrwInstruction(rd, rs1, csr),
                2 => new CsrrsInstruction(rd, rs1, csr),
                3 => new CsrrcInstruction(rd, rs1, csr),
                5 => new CsrrwiInstruction(rd, rs1, csr),
                _ => new NopInstruction()
            };
        });
    }
}

