using System;
using System.Collections.Generic;

namespace AetherRISC.Core.Assembler
{
    public static class RegisterAlias
    {
        private static readonly Dictionary<string, int> _intMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "zero", 0 }, { "ra", 1 }, { "sp", 2 }, { "gp", 3 }, { "tp", 4 },
                { "t0", 5 }, { "t1", 6 }, { "t2", 7 }, { "s0", 8 }, { "fp", 8 },
                { "s1", 9 }, { "a0", 10 }, { "a1", 11 }, { "a2", 12 }, { "a3", 13 },
                { "a4", 14 }, { "a5", 15 }, { "a6", 16 }, { "a7", 17 },
                { "s2", 18 }, { "s3", 19 }, { "s4", 20 }, { "s5", 21 }, { "s6", 22 },
                { "s7", 23 }, { "s8", 24 }, { "s9", 25 }, { "s10", 26 }, { "s11", 27 },
                { "t3", 28 }, { "t4", 29 }, { "t5", 30 }, { "t6", 31 },
            };

        private static readonly Dictionary<string, int> _fpMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "ft0", 0 }, { "ft1", 1 }, { "ft2", 2 }, { "ft3", 3 }, { "ft4", 4 },
                { "ft5", 5 }, { "ft6", 6 }, { "ft7", 7 }, { "fs0", 8 }, { "fs1", 9 },
                { "fa0", 10 }, { "fa1", 11 }, { "fa2", 12 }, { "fa3", 13 }, { "fa4", 14 },
                { "fa5", 15 }, { "fa6", 16 }, { "fa7", 17 }, { "fs2", 18 }, { "fs3", 19 },
                { "fs4", 20 }, { "fs5", 21 }, { "fs6", 22 }, { "fs7", 23 }, { "fs8", 24 },
                { "fs9", 25 }, { "fs10", 26 }, { "fs11", 27 }, { "ft8", 28 }, { "ft9", 29 },
                { "ft10", 30 }, { "ft11", 31 },
            };

        private static readonly Dictionary<string, uint> _csrMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "fflags", 0x001 },
                { "frm", 0x002 },
                { "fcsr", 0x003 },

                { "mstatus", 0x300 },
                { "misa", 0x301 },
                { "mie", 0x304 },
                { "mtvec", 0x305 },
                { "mscratch", 0x340 },
                { "mepc", 0x341 },
                { "mcause", 0x342 },
                { "mtval", 0x343 },
                { "mip", 0x344 },

                { "cycle", 0xC00 },
                { "time", 0xC01 },
                { "instret", 0xC02 },
            };

        public static bool TryParseCsr(string token, out uint csr)
        {
            csr = 0;
            token = token.Trim().TrimEnd(',');

            if (_csrMap.TryGetValue(token, out var mapped))
            {
                csr = mapped;
                return true;
            }

            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return uint.TryParse(
                    token.Substring(2),
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out csr
                );
            }

            return uint.TryParse(token, out csr);
        }

        public static int Parse(string token)
        {
            token = token.Trim().TrimEnd(',');

            // Hex literals are NOT registers
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"Not a register: {token}");

            // x0-x31
            if (
                token.StartsWith("x", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(token.Substring(1), out int xid)
                && xid >= 0
                && xid < 32
            )
                return xid;

            // f0-f31
            if (
                token.StartsWith("f", StringComparison.OrdinalIgnoreCase)
                && token.Length > 1
                && int.TryParse(token.Substring(1), out int fid)
                && fid >= 0
                && fid < 32
            )
                return fid;

            // Bare numbers 0-31 (legacy)
            if (int.TryParse(token, out int id) && id >= 0 && id < 32)
                return id;

            if (_intMap.TryGetValue(token, out int intAlias))
                return intAlias;

            if (_fpMap.TryGetValue(token, out int fpAlias))
                return fpAlias;

            // Important: CSR names must NOT parse here, so CSR operands fall through
            // to immediate parsing inside SourceAssembler.
            throw new FormatException($"Unknown register or alias: {token}");
        }
    }
}
