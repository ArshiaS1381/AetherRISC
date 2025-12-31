using System;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy
{
    public enum CacheAccessResult { Hit, Miss, ColdMiss }

    public class CacheLine
    {
        public bool Valid;
        public bool Dirty;
        public ulong Tag;
        public ulong LastAccessTick;
    }

    public class CacheSet
    {
        private readonly CacheLine[] _ways;
        private readonly ReplacementPolicy _policy;
        private readonly Random _rng = new();

        public CacheSet(int ways, ReplacementPolicy policy)
        {
            _ways = new CacheLine[ways];
            for (int i = 0; i < ways; i++) _ways[i] = new CacheLine();
            _policy = policy;
        }

        public CacheLine? Find(ulong tag)
        {
            for (int i = 0; i < _ways.Length; i++)
                if (_ways[i].Valid && _ways[i].Tag == tag) return _ways[i];
            return null;
        }

        public CacheLine GetVictim(ulong currentTick)
        {
            // Invalid lines are always first victims
            for (int i = 0; i < _ways.Length; i++)
                if (!_ways[i].Valid) return _ways[i];

            if (_policy == ReplacementPolicy.Random)
                return _ways[_rng.Next(_ways.Length)];

            // LRU
            CacheLine victim = _ways[0];
            ulong minTick = ulong.MaxValue;
            foreach (var way in _ways)
            {
                if (way.LastAccessTick < minTick)
                {
                    minTick = way.LastAccessTick;
                    victim = way;
                }
            }
            return victim;
        }
    }

    public class CacheController
    {
        private readonly CacheSet[] _sets;
        private readonly int _indexShift;
        private readonly int _setMask;
        private ulong _globalTick = 0;

        public int Latency { get; }
        public string Name { get; }
        
        private readonly WritePolicy _writePolicy;
        private readonly AllocationPolicy _allocPolicy;

        public CacheController(string name, CacheConfiguration config)
        {
            Name = name;
            Latency = config.LatencyCycles;
            _writePolicy = config.Write;
            _allocPolicy = config.Allocation;

            // Protection against bad config
            if (config.LineSizeBytes <= 0) config.LineSizeBytes = 64;
            if (config.Associativity <= 0) config.Associativity = 1;

            int numSets = Math.Max(1, config.SizeBytes / (config.Associativity * config.LineSizeBytes));
            _sets = new CacheSet[numSets];
            for (int i = 0; i < numSets; i++) 
                _sets[i] = new CacheSet(config.Associativity, config.Replacement);

            _indexShift = (int)Math.Log2(config.LineSizeBytes);
            _setMask = numSets - 1;
        }

        public void Tick() => _globalTick++;

        public CacheAccessResult Access(ulong addr, bool isWrite, out bool writebackRequired)
        {
            writebackRequired = false;
            ulong index = (addr >> _indexShift) & (ulong)_setMask;
            ulong tag = addr >> (_indexShift + (int)Math.Log2(_sets.Length));
            
            var set = _sets[index];
            var line = set.Find(tag);

            // --- HIT ---
            if (line != null)
            {
                line.LastAccessTick = _globalTick;
                if (isWrite)
                {
                    if (_writePolicy == WritePolicy.WriteBack)
                    {
                        line.Dirty = true;
                    }
                    else // WriteThrough
                    {
                        writebackRequired = true; 
                    }
                }
                return CacheAccessResult.Hit;
            }

            // --- MISS ---
            if (isWrite && _allocPolicy == AllocationPolicy.NoWriteAllocate)
            {
                return CacheAccessResult.Miss; // Bypass cache
            }

            // Allocation required
            var victim = set.GetVictim(_globalTick);
            
            if (victim.Valid && victim.Dirty)
            {
                writebackRequired = true; // Eviction writeback
            }

            victim.Valid = true;
            victim.Tag = tag;
            victim.LastAccessTick = _globalTick;
            
            if (isWrite && _writePolicy == WritePolicy.WriteBack)
                victim.Dirty = true;
            else 
                victim.Dirty = false;

            return CacheAccessResult.Miss;
        }
    }
}
