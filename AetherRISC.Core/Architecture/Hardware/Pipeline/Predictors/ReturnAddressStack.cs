using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class ReturnAddressStack
    {
        private readonly ulong[] _stack;
        private int _tos = 0; // Top of Stack
        private readonly int _capacity;
        private readonly int _mask;

        public ReturnAddressStack(int entries = 32)
        {
            _capacity = entries;
            _mask = entries - 1;
            _stack = new ulong[entries];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(ulong address)
        {
            _tos = (_tos + 1) & _mask;
            _stack[_tos] = address;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Pop()
        {
            ulong addr = _stack[_tos];
            _tos = (_tos - 1) & _mask;
            if (_tos < 0) _tos += _capacity; // Wrap around logic fix
            return addr;
        }

        public void Reset()
        {
            _tos = 0;
        }
    }
}
