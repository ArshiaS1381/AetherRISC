using System;

namespace AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy
{
    public enum CacheAccessResult { Hit, Miss }

    public class CacheSet
    {
        public class CacheLine
        {
            public bool Valid;
            public bool Dirty;
            public ulong Tag;
            public ulong LastAccessTick;
            public byte[] Data = Array.Empty<byte>();
        }

        private readonly CacheLine[] _ways;
        private readonly int _associativity;

        public CacheSet(int ways, int lineSize)
        {
            _associativity = ways;
            _ways = new CacheLine[ways];
            for (int i = 0; i < ways; i++) 
                _ways[i] = new CacheLine { Data = new byte[lineSize] };
        }

        public CacheLine? Find(ulong tag)
        {
            for (int i = 0; i < _associativity; i++)
                if (_ways[i].Valid && _ways[i].Tag == tag) return _ways[i];
            return null;
        }

        public CacheLine GetVictim(ulong currentTick)
        {
            CacheLine victim = _ways[0];
            ulong minTick = ulong.MaxValue;

            foreach (var way in _ways)
            {
                if (!way.Valid) return way; 
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
        private ulong _globalTick = 0;

        public int Latency { get; }
        public string Name { get; }

        public CacheController(string name, int sizeBytes, int ways, int lineSize, int latency)
        {
            Name = name;
            Latency = latency;
            _lineSize = lineSize;
            
            int numSets = Math.Max(1, sizeBytes / (ways * lineSize));
            _sets = new CacheSet[numSets];
            for (int i = 0; i < numSets; i++) 
                _sets[i] = new CacheSet(ways, lineSize);

            _indexShift = (int)Math.Log2(lineSize);
            _setMask = numSets - 1;
        }

        public void Tick() => _globalTick++;

        public CacheAccessResult Access(ulong addr, bool isWrite, out bool evictionOccurred)
        {
            evictionOccurred = false;
            ulong index = (addr >> _indexShift) & (ulong)_setMask;
            ulong tag = addr >> (_indexShift + (int)Math.Log2(_sets.Length));
            
            var set = _sets[index];
            var line = set.Find(tag);

            if (line != null)
            {
                line.LastAccessTick = _globalTick;
                if (isWrite) line.Dirty = true;
                return CacheAccessResult.Hit;
            }

            var victim = set.GetVictim(_globalTick);
            if (victim.Valid) evictionOccurred = true;

            victim.Valid = true;
            victim.Tag = tag;
            victim.Dirty = isWrite; 
            victim.LastAccessTick = _globalTick;
            
            return CacheAccessResult.Miss;
        }
    }
}
