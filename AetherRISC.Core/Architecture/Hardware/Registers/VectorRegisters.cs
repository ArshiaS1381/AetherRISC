using System;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.Hardware.Registers
{
    public class VectorRegisters
    {
        // 32 Registers * VLEN bits
        private readonly byte[][] _regs;
        public int VLen { get; }      // Bits
        public int VLenBytes { get; } // Bytes
        
        // --- Vector CSRs ---
        public ulong Vstart { get; set; }
        public ulong Vxsat { get; set; }
        public ulong Vxrm { get; set; }
        public ulong Vcsr { get; set; }
        
        // VTYPE fields
        public ulong Vtype { get; set; }
        public int Vl { get; set; } // Vector Length (active elements)
        
        // Derived from VTYPE
        public int SewBytes { get; private set; } = 1; // Standard Element Width (Bytes)
        public float Lmul { get; private set; } = 1.0f; // Register Grouping

        public VectorRegisters(int vlenBits = 128)
        {
            VLen = vlenBits;
            VLenBytes = vlenBits / 8;
            _regs = new byte[32][];
            for (int i = 0; i < 32; i++) _regs[i] = new byte[VLenBytes];
        }

        public void UpdateVtype(ulong newVtype, int avl)
        {
            Vtype = newVtype;
            
            // Decode SEW (bits 3-5)
            // 000=8, 001=16, 010=32, 011=64
            int sewCode = (int)((newVtype >> 3) & 0x7);
            SewBytes = 1 << sewCode;

            // Decode LMUL (bits 0-2)
            // 000=1, 001=2, 010=4, 011=8, 111=0.5, 110=0.25, 101=0.125
            int lmulCode = (int)(newVtype & 0x7);
            switch(lmulCode) {
                case 0: Lmul = 1; break;
                case 1: Lmul = 2; break;
                case 2: Lmul = 4; break;
                case 3: Lmul = 8; break;
                case 5: Lmul = 0.125f; break;
                case 6: Lmul = 0.25f; break;
                case 7: Lmul = 0.5f; break;
            }

            // Calculate VL (simplified)
            // VLMAX = (VLEN / SEW) * LMUL
            int vlmax = (int)((VLen / (SewBytes * 8)) * Lmul);
            if (vlmax < 0) vlmax = 0; // Guard against f32 lmul issues for now

            Vl = (avl < vlmax) ? avl : vlmax;
        }

        public byte[] GetRaw(int index) => _regs[index];
        
        public void WriteRaw(int index, byte[] data)
        {
             if(index >= 0 && index < 32) 
                Array.Copy(data, _regs[index], Math.Min(data.Length, VLenBytes));
        }

        public void Reset() 
        { 
            for (int i = 0; i < 32; i++) Array.Clear(_regs[i], 0, VLenBytes); 
            Vstart = 0; Vl = 0;
        }
    }
}
