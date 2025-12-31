namespace AetherRISC.Core.Abstractions.Interfaces
{
    // Describes the Translation Lookaside Buffer
    public interface ITlb
    {
        void Flush();
        bool Lookup(ulong vAddr, out ulong pAddr);
        void Insert(ulong vAddr, ulong pAddr);
    }

    // Describes the MMU
    public interface IMmu
    {
        bool Enabled { get; set; }
        ulong RootPageTablePointer { get; set; }
        ulong Translate(ulong vAddr, bool isWrite);
        void HandlePageFault(ulong vAddr);
    }

    // Describes a Cache Level (L1, L2, etc)
    public interface ICache
    {
        string Name { get; }
        int SizeBytes { get; }
        int Associativity { get; }
        int LineSizeBytes { get; }
        
        bool TryRead(ulong addr, out byte[] data); // Returns false on miss
        void Write(ulong addr, byte[] data);
    }

    // Extends the existing IMemoryBus to support hierarchy
    public interface IMemoryHierarchy : IMemoryBus
    {
        ICache? L1Instruction { get; }
        ICache? L1Data { get; }
        IMmu? Mmu { get; }
    }
}
