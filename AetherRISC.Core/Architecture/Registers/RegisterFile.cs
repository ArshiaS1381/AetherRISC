using System;

namespace AetherRISC.Core.Architecture.Registers
{
    public class RegisterFile
    {
        private readonly ulong[] _registers = new ulong[32];

        // We assume 64-bit storage always.
        // In 32-bit mode, we just ensure values are sign-extended or truncated appropriately
        // usually handled by the Instruction Execute logic, but we can enforce it here too.

        public ulong Read(int index)
        {
            if (index == 0) return 0;
            if (index < 0 || index >= 32) return 0;
            return _registers[index];
        }

        public void Write(int index, ulong value)
        {
            if (index == 0) return; // x0 is always hardwired to 0
            if (index < 0 || index >= 32) return;

            // Note: In a rigorous emulator, we might pass the SystemConfig here 
            // to truncate 32-bit values if we were in RV32 mode.
            // However, our instructions (ADD, SUB, etc.) are already handling the truncation logic
            // before calling Write(). So we can just store the raw bits here.
            
            _registers[index] = value;
        }

        // Helper for debugging/tests
        public ulong[] Dump() => (ulong[])_registers.Clone();
    }
}
