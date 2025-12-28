using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;

using AetherRISC.Core.Assembler;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

public partial class InstructionDecoder
{
    private readonly List<DecoderEntry> _factories = new();
    private record DecoderEntry(uint Op, uint? F3, uint? F7, Func<uint, IInstruction?> Factory);

    public InstructionDecoder() : this(InstructionSet.All) { }

    public InstructionDecoder(InstructionSet enabledSets)
    {
        RegisterGeneratedOpcodes(enabledSets);
    }

    partial void RegisterGeneratedOpcodes(InstructionSet enabledSets);

    private void RegisterOpcode(uint op, Func<uint, IInstruction?> factory) =>
        _factories.Add(new DecoderEntry(op, null, null, factory));

    public IInstruction? Decode(uint raw)
    {
        uint opcode = raw & 0x7F;

        foreach (var entry in _factories)
        {
            if (entry.Op != opcode) continue;
            var result = entry.Factory(raw);
            if (result != null) return result;
        }

        return DecodeReflection(raw, opcode);
    }

    private IInstruction? DecodeReflection(uint raw, uint opcode)
    {
        uint funct3 = (raw >> 12) & 0x7;
        uint funct7 = (raw >> 25) & 0x7F;

        var assembly = Assembly.GetExecutingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(IInstruction).IsAssignableFrom(type) || type.IsAbstract) continue;

            var attr = type.GetCustomAttribute<RiscvInstructionAttribute>();
            if (attr == null || attr.Opcode != opcode) continue;

            bool f3Match = attr.Type is RiscvEncodingType.U or RiscvEncodingType.J || attr.Funct3 == funct3;
            bool f7Match = attr.Type is not (RiscvEncodingType.R or RiscvEncodingType.ZbbUnary) || attr.Funct7 == funct7;

            if (attr.Type == RiscvEncodingType.ZbbUnary)
            {
                uint rs2 = (raw >> 20) & 0x1F;
                if (rs2 != attr.Rs2Sel) continue;
            }

            if (f3Match && f7Match)
                return Instantiate(type, raw, attr.Type);
        }
        return null;
    }

    private static IInstruction Instantiate(Type t, uint raw, RiscvEncodingType encoding)
    {
        int rd = (int)((raw >> 7) & 0x1F);
        int rs1 = (int)((raw >> 15) & 0x1F);
        int rs2 = (int)((raw >> 20) & 0x1F);

        return encoding switch
        {
            RiscvEncodingType.R => (IInstruction)Activator.CreateInstance(t, rd, rs1, rs2)!,
            RiscvEncodingType.I => (IInstruction)Activator.CreateInstance(t, rd, rs1, BitUtils.ExtractITypeImm(raw))!,
            RiscvEncodingType.S => (IInstruction)Activator.CreateInstance(t, rs1, rs2, BitUtils.ExtractSTypeImm(raw))!,
            RiscvEncodingType.B => (IInstruction)Activator.CreateInstance(t, rs1, rs2, BitUtils.ExtractBTypeImm(raw))!,
            RiscvEncodingType.U => (IInstruction)Activator.CreateInstance(t, rd, BitUtils.ExtractUTypeImm(raw))!,
            RiscvEncodingType.J => (IInstruction)Activator.CreateInstance(t, rd, BitUtils.ExtractJTypeImm(raw))!,
            RiscvEncodingType.ShiftImm => (IInstruction)Activator.CreateInstance(t, rd, rs1, (int)(raw >> 20) & 0x3F)!,
            RiscvEncodingType.ZbbUnary => (IInstruction)Activator.CreateInstance(t, rd, rs1, 0)!,
            _ => (IInstruction)Activator.CreateInstance(t, rd, rs1, rs2)!
        };
    }
}
