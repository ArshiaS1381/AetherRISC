using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Architecture.ISA.Families;

public class Rv64iFamily : IInstructionFamily
{
    public void Register(InstructionDecoder decoder)
    {
        RegisterEncoding();
        RegisterDecoding(decoder);
    }

    private void RegisterEncoding()
    {
        // R-Type
        InstructionEncoder.Register("ADD", i => InstructionEncoder.GenR(0x33, 0, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("SUB", i => InstructionEncoder.GenR(0x33, 0, 32, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("SLL", i => InstructionEncoder.GenR(0x33, 1, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("SLT", i => InstructionEncoder.GenR(0x33, 2, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("SLTU", i => InstructionEncoder.GenR(0x33, 3, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("XOR", i => InstructionEncoder.GenR(0x33, 4, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("SRL", i => InstructionEncoder.GenR(0x33, 5, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("SRA", i => InstructionEncoder.GenR(0x33, 5, 32, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("OR", i => InstructionEncoder.GenR(0x33, 6, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));
        InstructionEncoder.Register("AND", i => InstructionEncoder.GenR(0x33, 7, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Rs2));

        // I-Type
        InstructionEncoder.Register("ADDI", i => InstructionEncoder.GenI(0x13, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("SLLI", i => InstructionEncoder.GenI(0x13, 1, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("SRLI", i => InstructionEncoder.GenI(0x13, 5, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("SRAI", i => InstructionEncoder.GenI(0x13, 5, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm | 0x400)); // Bit 30 set for SRAI
        InstructionEncoder.Register("LD", i => InstructionEncoder.GenI(0x03, 3, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("LW", i => InstructionEncoder.GenI(0x03, 2, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("JALR", i => InstructionEncoder.GenI(0x67, 0, (uint)i.Rd, (uint)i.Rs1, (uint)i.Imm));
        InstructionEncoder.Register("ECALL", _ => 0x00000073);

        // B-Type
        InstructionEncoder.Register("BEQ", i => InstructionEncoder.GenB(0x63, 0, (uint)i.Rs1, (uint)i.Rs2, (uint)i.Imm));
        InstructionEncoder.Register("BNE", i => InstructionEncoder.GenB(0x63, 1, (uint)i.Rs1, (uint)i.Rs2, (uint)i.Imm));
        InstructionEncoder.Register("BLT", i => InstructionEncoder.GenB(0x63, 4, (uint)i.Rs1, (uint)i.Rs2, (uint)i.Imm));
        InstructionEncoder.Register("BGE", i => InstructionEncoder.GenB(0x63, 5, (uint)i.Rs1, (uint)i.Rs2, (uint)i.Imm));

        // J-Type
        InstructionEncoder.Register("JAL", i => InstructionEncoder.GenJ(0x6F, (uint)i.Rd, (uint)i.Imm));

        // S-Type
        InstructionEncoder.Register("SD", i => InstructionEncoder.GenS(0x23, 3, (uint)i.Rs1, (uint)i.Rs2, (uint)i.Imm));
        InstructionEncoder.Register("SW", i => InstructionEncoder.GenS(0x23, 2, (uint)i.Rs1, (uint)i.Rs2, (uint)i.Imm));
        
        // U-Type
        InstructionEncoder.Register("LUI", i => InstructionEncoder.GenU(0x37, (uint)i.Rd, (uint)i.Imm));
        InstructionEncoder.Register("AUIPC", i => InstructionEncoder.GenU(0x17, (uint)i.Rd, (uint)i.Imm));
    }

    private void RegisterDecoding(InstructionDecoder decoder)
    {
        // 0x33: BASE OPS
        decoder.RegisterOpcode(0x33, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int f7 = (int)((inst >> 25) & 0x7F);
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int rs2 = (int)((inst >> 20) & 0x1F);
            
            if (f7 != 0 && f7 != 32) return new NopInstruction(); 

            switch(f3) {
                case 0: return (f7 == 32 ? (IInstruction)new SubInstruction(rd, rs1, rs2) : new AddInstruction(rd, rs1, rs2));
                case 1: return new SllInstruction(rd, rs1, rs2);
                case 2: return new SltInstruction(rd, rs1, rs2);
                case 3: return new SltuInstruction(rd, rs1, rs2);
                case 4: return new XorInstruction(rd, rs1, rs2);
                case 5: return (f7 == 32) ? (IInstruction)new SraInstruction(rd, rs1, rs2) : new SrlInstruction(rd, rs1, rs2);
                case 6: return new OrInstruction(rd, rs1, rs2);
                case 7: return new AndInstruction(rd, rs1, rs2);
                default: return new NopInstruction();
            }
        });

        // 0x63: BRANCH
        decoder.RegisterOpcode(0x63, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int rs2 = (int)((inst >> 20) & 0x1F);
            int imm = BitUtils.ExtractBTypeImm(inst);
            return f3 switch {
                0 => new BeqInstruction(rs1, rs2, imm, 0),
                1 => new BneInstruction(rs1, rs2, imm, 1),
                4 => new BltInstruction(rs1, rs2, imm),
                5 => new BgeInstruction(rs1, rs2, imm),
                _ => new NopInstruction()
            };
        });

        // 0x13: OP-IMM
        decoder.RegisterOpcode(0x13, (inst) => {
            int f3 = (int)((inst >> 12) & 0x7);
            int rd = (int)((inst >> 7) & 0x1F);
            int rs1 = (int)((inst >> 15) & 0x1F);
            int imm = BitUtils.ExtractITypeImm(inst);
            switch(f3) {
                case 0: return new AddiInstruction(rd, rs1, imm);
                case 1: return new SlliInstruction(rd, rs1, imm);
                case 2: return new SltiInstruction(rd, rs1, imm);
                case 3: return new SltiuInstruction(rd, rs1, imm);
                case 4: return new XoriInstruction(rd, rs1, imm);
                case 5: return (inst & 0x40000000) != 0 ? (IInstruction)new SraiInstruction(rd, rs1, imm) : new SrliInstruction(rd, rs1, imm);
                case 6: return new OriInstruction(rd, rs1, imm);
                case 7: return new AndiInstruction(rd, rs1, imm);
                default: return new AddiInstruction(rd, rs1, imm);
            }
        });
        
        // 0x03: LOAD
        decoder.RegisterOpcode(0x03, (inst) => {
             int f3 = (int)((inst >> 12) & 0x7);
             int rd = (int)((inst >> 7) & 0x1F);
             int rs1 = (int)((inst >> 15) & 0x1F);
             int imm = BitUtils.ExtractITypeImm(inst);
             return f3 == 2 ? (IInstruction)new LwInstruction(rd, rs1, imm) : new LdInstruction(rd, rs1, imm);
        });
        
        // 0x23: STORE
        decoder.RegisterOpcode(0x23, (inst) => {
             int f3 = (int)((inst >> 12) & 0x7);
             int rs1 = (int)((inst >> 15) & 0x1F);
             int rs2 = (int)((inst >> 20) & 0x1F);
             int imm = BitUtils.ExtractSTypeImm(inst);
             return f3 == 2 ? (IInstruction)new SwInstruction(rs1, rs2, imm) : new SdInstruction(rs1, rs2, imm);
        });

        decoder.RegisterOpcode(0x6F, (inst) => {
             int rd = (int)((inst >> 7) & 0x1F);
             uint i = inst;
             int imm20 = (int)(((i>>31)&1)<<20 | ((i>>12)&0xFF)<<12 | ((i>>20)&1)<<11 | ((i>>21)&0x3FF)<<1);
             if ((imm20 & 0x100000) != 0) imm20 |= unchecked((int)0xFFE00000); 
             return new JalInstruction(rd, imm20);
        });
        
        decoder.RegisterOpcode(0x73, _ => new EcallInstruction());
        decoder.RegisterOpcode(0x37, (inst) => { int rd = (int)((inst >> 7) & 0x1F); int imm = (int)(inst & 0xFFFFF000); return new LuiInstruction(rd, imm); });
        decoder.RegisterOpcode(0x17, (inst) => { int rd = (int)((inst >> 7) & 0x1F); int imm = (int)(inst & 0xFFFFF000); return new AuipcInstruction(rd, imm); });
    }
}
