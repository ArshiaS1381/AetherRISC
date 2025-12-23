using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Extensions.F;
using AetherRISC.Core.Architecture.ISA.Extensions.D;

namespace AetherRISC.Core.Architecture.ISA.Families;

public class RvFloatComputeFamily : IInstructionFamily
{
    public void Register(InstructionDecoder decoder)
    {
        decoder.RegisterOpcode(0x53, (inst) => {
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int rs2 = (int)((inst >> 20) & 0x1F);
            int funct7 = (int)((inst >> 25) & 0x7F);
            int fmt = funct7 & 0x03; 
            int op = funct7 >> 2;

            if (fmt == 0) { // Single Precision
                return op switch {
                    0x00 => new FaddSInstruction(rd, rs1, rs2),
                    0x01 => new FsubSInstruction(rd, rs1, rs2),
                    0x02 => new FmulSInstruction(rd, rs1, rs2),
                    0x03 => new FdivSInstruction(rd, rs1, rs2),
                    0x04 => new FsgnjSInstruction(rd, rs1, rs2),
                    0x05 => new FsgnjnSInstruction(rd, rs1, rs2),
                    0x06 => new FsgnjxSInstruction(rd, rs1, rs2),
                    0x07 => new FminSInstruction(rd, rs1, rs2),
                    0x08 => new FmaxSInstruction(rd, rs1, rs2),
                    0x0B => new FsqrtSInstruction(rd, rs1),
                    0x14 => new FleSInstruction(rd, rs1, rs2),
                    0x15 => new FltSInstruction(rd, rs1, rs2),
                    0x16 => new FeqSInstruction(rd, rs1, rs2),
                    0x09 => new FcvtDSInstruction(rd, rs1),
                    0x18 => new FcvtWSInstruction(rd, rs1),
                    0x1A => new FcvtSWInstruction(rd, rs1),
                    0x1C => new FclassSInstruction(rd, rs1),
                    0x1E => new FmvXWInstruction(rd, rs1),
                    0x1F => new FmvWXInstruction(rd, rs1),
                    _    => new NopInstruction()
                };
            } 
            else if (fmt == 1) { // Double Precision
                return op switch {
                    0x00 => new FaddDInstruction(rd, rs1, rs2),
                    0x01 => new FsubDInstruction(rd, rs1, rs2),
                    0x02 => new FmulDInstruction(rd, rs1, rs2),
                    0x03 => new FdivDInstruction(rd, rs1, rs2),
                    0x04 => new FsgnjDInstruction(rd, rs1, rs2),
                    0x05 => new FsgnjnDInstruction(rd, rs1, rs2),
                    0x06 => new FsgnjxDInstruction(rd, rs1, rs2),
                    0x07 => new FminDInstruction(rd, rs1, rs2),
                    0x08 => new FmaxDInstruction(rd, rs1, rs2),
                    0x0B => new FsqrtDInstruction(rd, rs1),
                    0x14 => new FleDInstruction(rd, rs1, rs2),
                    0x15 => new FltDInstruction(rd, rs1, rs2),
                    0x16 => new FeqDInstruction(rd, rs1, rs2),
                    0x20 => new FcvtSDInstruction(rd, rs1),
                    0x18 when rs2 == 0 => new FcvtWDInstruction(rd, rs1),
                    0x18 when rs2 == 2 => new FcvtLDInstruction(rd, rs1),
                    0x1A => new FcvtDWInstruction(rd, rs1),
                    0x1C => new FclassDInstruction(rd, rs1),
                    0x1E => new FmvXDInstruction(rd, rs1),
                    _    => new NopInstruction()
                };
            }

            return new NopInstruction();
        });
    }
}

