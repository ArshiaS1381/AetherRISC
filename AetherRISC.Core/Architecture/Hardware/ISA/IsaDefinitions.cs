using System;

namespace AetherRISC.Core.Architecture.Hardware.ISA
{
    [Flags]
    public enum InstructionSet
    {
        None        = 0,
        RV32I       = 1 << 0,
        RV64I       = 1 << 1,
        RV64M       = 1 << 2,
        RV64A       = 1 << 3,
        RV64F       = 1 << 4,
        RV64D       = 1 << 5,
        RV64C       = 1 << 6,
        Zicsr       = 1 << 7,
        Zifencei    = 1 << 8,
        Zba         = 1 << 9,
        Zbb         = 1 << 10,
        Zbc         = 1 << 11,
        Zbs         = 1 << 12,
        RV64G       = RV64I | RV64M | RV64A | RV64F | RV64D,
        All         = ~0
    }

    public enum RiscvEncodingType { R, I, S, B, U, J, R4, ShiftImm, ZbbUnary, Custom }

    public struct InstructionData
    {
        public int Rd;
        public int Rs1;
        public int Rs2;
        public int Imm;
        public ulong Immediate; 
        public ulong PC;        
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RiscvInstructionAttribute : Attribute
    {
        public string Mnemonic { get; }
        public InstructionSet Architecture { get; }
        
        // FIX: The property MUST be named 'Type' for the generator to work
        public RiscvEncodingType Type { get; }
        
        public uint Opcode { get; }
        
        public uint Funct3 { get; set; }
        public uint Funct7 { get; set; }
        public uint Funct6 { get; set; } 
        public uint Rs2Sel { get; set; } 

        public string Name { get; set; } = "Unknown";
        public string Description { get; set; } = "";
        public string Usage { get; set; } = "";

        public RiscvInstructionAttribute(string mnemonic, InstructionSet arch, RiscvEncodingType type, uint opcode)
        {
            Mnemonic = mnemonic;
            Architecture = arch;
            Type = type;
            Opcode = opcode;
        }
    }
}
