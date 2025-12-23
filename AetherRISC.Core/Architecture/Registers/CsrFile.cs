using System.Collections.Generic;

namespace AetherRISC.Core.Architecture.Registers
{
    public class CsrFile
    {
        private readonly Dictionary<uint, ulong> _csrs = new Dictionary<uint, ulong>();

        public ulong Read(uint address)
        {
            // If strictly following spec, we should check permissions (User/Machine) here.
            // For now, we allow full access.
            if (_csrs.TryGetValue(address, out var val)) return val;
            return 0;
        }

        public void Write(uint address, ulong value)
        {
            // Some CSRs are Read-Only (like time, cycle usually hardwired) or have specific bitmasks.
            // For this emulator phase, we treat them as simple storage.
            _csrs[address] = value;
        }
    }
}
