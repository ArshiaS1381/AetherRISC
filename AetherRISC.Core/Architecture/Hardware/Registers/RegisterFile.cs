using System;
using System.Runtime.CompilerServices; // Required for Inlining

namespace AetherRISC.Core.Architecture.Hardware.Registers
{
    public class RegisterFile
    {
        private readonly ulong[] _registers = new ulong[32];
        
        public ulong PC { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Read(int index)
        {
            // x0 is always 0. The array entry is 0 initialized, 
            // but the check prevents logic errors elsewhere.
            if (index == 0) return 0;
            return _registers[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int index, ulong value)
        {
            if (index != 0) _registers[index] = value;
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
            Array.Clear(_registers, 0, _registers.Length);
            PC = 0;
        }
    }
}
