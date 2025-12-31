using System;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy
{
    public class CacheLine
    {
        public bool Valid;
        public bool Dirty;
        public ulong Tag;
        public ulong LastAccessTick;
        public byte[] Data; // Actual data storage

        public CacheLine(int size) { Data = new byte[size]; }
    }

    public class CacheSet
    {
        private readonly CacheLine[] _ways;
        private readonly ReplacementPolicy _policy;
        private readonly Random _rng = new();

        public CacheSet(int ways, int lineSize, ReplacementPolicy policy)
        {
            _ways = new CacheLine[ways];
            for (int i = 0; i < ways; i++) _ways[i] = new CacheLine(lineSize);
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
            // Invalid lines first
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
        private readonly int _lineSize;
        private readonly int _indexShift;
        private readonly int _setMask;
        private readonly int _tagShift;
        private ulong _globalTick = 0;

        public int Latency { get; }
        public string Name { get; }
        public WritePolicy WritePolicy { get; }
        public AllocationPolicy AllocPolicy { get; }
        
        public CacheController(string name, CacheConfiguration config)
        {
            Name = name;
            Latency = config.LatencyCycles;
            WritePolicy = config.Write;
            AllocPolicy = config.Allocation;
            _lineSize = config.LineSizeBytes;

            if (config.LineSizeBytes <= 0) config.LineSizeBytes = 64;
            if (config.Associativity <= 0) config.Associativity = 1;

            int numSets = Math.Max(1, config.SizeBytes / (config.Associativity * config.LineSizeBytes));
            _sets = new CacheSet[numSets];
            for (int i = 0; i < numSets; i++) 
                _sets[i] = new CacheSet(config.Associativity, config.LineSizeBytes, config.Replacement);

            _indexShift = (int)Math.Log2(config.LineSizeBytes);
            _setMask = numSets - 1;
            _tagShift = _indexShift + (int)Math.Log2(numSets);
        }

        public void Tick() => _globalTick++;

        // Returns true on Hit
        public bool TryRead(uint address, out byte val)
        {
            ulong index = (address >> _indexShift) & (ulong)_setMask;
            ulong tag = address >> _tagShift;
            var line = _sets[index].Find(tag);

            if (line != null)
            {
                line.LastAccessTick = _globalTick;
                val = line.Data[address & (_lineSize - 1)];
                return true;
            }
            val = 0;
            return false;
        }

        // Returns true on Hit, false on Miss
        public bool TryWrite(uint address, byte val)
        {
            ulong index = (address >> _indexShift) & (ulong)_setMask;
            ulong tag = address >> _tagShift;
            var line = _sets[index].Find(tag);

            if (line != null)
            {
                line.LastAccessTick = _globalTick;
                line.Data[address & (_lineSize - 1)] = val;
                if (WritePolicy == WritePolicy.WriteBack) line.Dirty = true;
                return true;
            }
            return false;
        }

        public void Fill(uint address, byte[] data, out ulong? evictedAddr, out byte[]? evictedData)
        {
            evictedAddr = null;
            evictedData = null;
            
            ulong index = (address >> _indexShift) & (ulong)_setMask;
            ulong tag = address >> _tagShift;
            var set = _sets[index];
            var line = set.GetVictim(_globalTick);

            if (line.Valid && line.Dirty)
            {
                // Reconstruct address
                evictedAddr = (line.Tag << _tagShift) | (index << _indexShift);
                evictedData = (byte[])line.Data.Clone();
            }

            line.Valid = true;
            line.Dirty = false; // Fresh fill is clean
            line.Tag = tag;
            line.LastAccessTick = _globalTick;
            Array.Copy(data, line.Data, _lineSize);
        }
    }
}
