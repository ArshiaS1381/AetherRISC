using System;
using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Decoding
{
    public class DecodedCacheEntry
    {
        public IInstruction Inst;
        public int Rd, Rs1, Rs2, Imm;
        public bool IsLoad, IsStore, IsBranch, IsJump, IsFloatRegWrite, RegWrite;

        public DecodedCacheEntry(IInstruction inst)
        {
            Inst = inst;
            Rd = inst?.Rd ?? 0;
            Rs1 = inst?.Rs1 ?? 0;
            Rs2 = inst?.Rs2 ?? 0;
            Imm = inst?.Imm ?? 0;
            IsLoad = inst?.IsLoad ?? false;
            IsStore = inst?.IsStore ?? false;
            IsBranch = inst?.IsBranch ?? false;
            IsJump = inst?.IsJump ?? false;
            IsFloatRegWrite = inst?.IsFloatRegWrite ?? false;
            RegWrite = (Rd != 0 || IsFloatRegWrite) && !IsStore && !IsBranch;
        }
    }

    public partial class InstructionDecoder
    {
        private static readonly Dictionary<uint, DecodedCacheEntry> _fastCache = new();
        private static readonly DecodedCacheEntry _sentinel;
        private readonly InstructionSet _enabledSets;
        private readonly ArchitectureSettings? _settings;

        static InstructionDecoder() 
        { 
            _sentinel = new DecodedCacheEntry(null!); 
        }

        public InstructionDecoder(InstructionSet enabledSets = InstructionSet.All, ArchitectureSettings? settings = null)
        {
            _enabledSets = enabledSets;
            _settings = settings;
        }

        public InstructionDecoder() : this(InstructionSet.All, null) { }

        private partial IInstruction? DecodeGenerated(uint raw, InstructionSet enabledSets);

        public DecodedCacheEntry? DecodeFast(uint raw)
        {
            if (raw == 0) return null;
            if (_fastCache.TryGetValue(raw, out var cached)) return ReferenceEquals(cached, _sentinel) ? null : cached;
            IInstruction? inst = PerformFullDecode(raw);
            var entry = (inst == null) ? _sentinel : new DecodedCacheEntry(inst);
            _fastCache[raw] = entry;
            return (inst == null) ? null : entry;
        }
        
        public IInstruction? Decode(uint raw) => DecodeFast(raw)?.Inst;

        private IInstruction? PerformFullDecode(uint raw)
        {
            var inst = DecodeGenerated(raw, _enabledSets) ?? DecodeManual(raw);
            if (inst != null && _settings != null)
            {
                if (_settings.DisabledInstructions.Contains(inst.Mnemonic)) return null;
            }
            return inst;
        }
    }
}
