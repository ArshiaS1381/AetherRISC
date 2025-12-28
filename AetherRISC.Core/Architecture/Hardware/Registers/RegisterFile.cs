using System;

namespace AetherRISC.Core.Architecture.Hardware.Registers
{
    public class RegisterFile
    {
        private readonly ulong[] _registers = new ulong[32];
        
        // The Program Counter
        public ulong PC { get; set; }

        // 1. Restore the Read() method required by legacy instructions
        public ulong Read(int index)
        {
            if (index == 0) return 0;
            return _registers[index];
        }

        // 2. Restore the Write() method required by legacy instructions
        public void Write(int index, ulong value)
        {
            if (index != 0) _registers[index] = value;
        }

        // 3. Keep the indexer for cleaner new code (state.Registers[5])
        public ulong this[int index]
        {
            get => Read(index);
            set => Write(index, value);
        }

        public void Reset()
        {
            Array.Clear(_registers, 0, _registers.Length);
            PC = 0;
        }
    }
}
