using System;

namespace AetherRISC.Core.Architecture.Registers
{
    public class FloatingPointRegisters
    {
        private readonly ulong[] _regs = new ulong[32];

        public ulong Read(int index) => _regs[index];
        public void Write(int index, ulong value) => _regs[index] = value;
        
        // Overload to handle raw bits (used by Load instructions)
        public void WriteSingle(int index, uint value)
        {
             _regs[index] = 0xFFFFFFFF00000000UL | (ulong)value;
        }

        // NEW: Overload to handle actual float values (used by Compute instructions)
        public void WriteSingle(int index, float value)
        {
            uint bits = BitConverter.SingleToUInt32Bits(value);
            WriteSingle(index, bits);
        }

        public float ReadSingle(int index)
        {
            return BitConverter.UInt32BitsToSingle((uint)(_regs[index] & 0xFFFFFFFF));
        }

        public void WriteDouble(int index, double value) 
            => _regs[index] = BitConverter.DoubleToUInt64Bits(value);

        public double ReadDouble(int index)
            => BitConverter.UInt64BitsToDouble(_regs[index]);
    }
}
