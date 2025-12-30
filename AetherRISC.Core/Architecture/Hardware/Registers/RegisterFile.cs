using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AetherRISC.Core.Architecture.Hardware.Registers
{
    public unsafe class RegisterFile
    {
        // Unmanaged memory pointer for true zero-overhead access
        private readonly ulong* _regs;
        
        public ulong PC { get; set; }

        public RegisterFile()
        {
            // Allocate 32 * 8 bytes (256 bytes) unmanaged
            _regs = (ulong*)NativeMemory.Alloc(32 * sizeof(ulong));
            Reset();
        }

        ~RegisterFile()
        {
            NativeMemory.Free(_regs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Read(int index)
        {
            // 0x1F mask ensures safety and handles 0-31 wrapping
            return _regs[index & 0x1F];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int index, ulong value)
        {
            if (index == 0) return;
            _regs[index & 0x1F] = value;
        }

        public ulong this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Write(index, value);
        }

        public void Reset()
        {
            NativeMemory.Clear(_regs, 32 * sizeof(ulong));
            PC = 0;
        }
    }
}
