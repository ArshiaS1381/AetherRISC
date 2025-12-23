using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Architecture.ISA.Extensions.Zifencei;

namespace AetherRISC.Core.Architecture.ISA.Families;

public class Rv64ZifenceiFamily : IInstructionFamily
{
    public void Register(InstructionDecoder decoder)
    {
        // Encoder: MISC-MEM (0x0F), Funct3=001 (FENCE.I)
        InstructionEncoder.Register("FENCE.I", i => InstructionEncoder.GenI(0x0F, 1, 0, 0, 0));

        // Decoder
        decoder.RegisterOpcode(0x0F, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            return f3 == 1 ? new FenceIInstruction() : new NopInstruction(); 
        });
    }
}
