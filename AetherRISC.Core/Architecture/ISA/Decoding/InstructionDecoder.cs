using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.ISA.Families;
using System.Collections.Generic;
using System;

namespace AetherRISC.Core.Architecture.ISA.Decoding;

public class InstructionDecoder
{
    private readonly Dictionary<uint, Func<uint, IInstruction?>> _opcodeMap;

    public InstructionDecoder()
    {
        _opcodeMap = new Dictionary<uint, Func<uint, IInstruction?>>();
        
        // Register Core Families
        new Rv64iFamily().Register(this);
        new Rv64mFamily().Register(this);
        new Rv64ZicsrFamily().Register(this);
        new Rv64aFamily().Register(this); // ATOMICS
        new Rv64ZifenceiFamily().Register(this); // NEW
    }

    public void RegisterOpcode(uint opcode, Func<uint, IInstruction> factory) 
    {
        if (!_opcodeMap.ContainsKey(opcode))
        {
            _opcodeMap[opcode] = (inst) => factory(inst);
        }
        else 
        {
            var previous = _opcodeMap[opcode];
            _opcodeMap[opcode] = (inst) => {
                var res = factory(inst);
                if (res is NopInstruction) return previous(inst);
                return res;
            };
        }
    }

    public IInstruction Decode(uint inst)
    {
        if (inst == 0) return new NopInstruction();
        uint opcode = inst & 0x7F;
        if (_opcodeMap.TryGetValue(opcode, out var factory)) 
        {
            var res = factory(inst);
            return res ?? new NopInstruction();
        }
        return new NopInstruction(); 
    }
}




