using System;

namespace AetherRISC.Core.Architecture.Hardware.Registers
{
    public class VectorRegisters
    {
        private readonly byte[][] _regs;
        public int VLen { get; }      
        public int VLenBytes { get; } 
        
        public ulong Vstart { get; set; }
        public ulong Vtype { get; set; }
        public int Vl { get; set; } 
        
        // Decoded
        public int SewBytes { get; private set; } = 1; // 1, 2, 4, 8
        public float Lmul { get; private set; } = 1.0f; 

        public VectorRegisters(int vlenBits = 128)
        {
            VLen = vlenBits;
            VLenBytes = vlenBits / 8;
            _regs = new byte[32][];
            for (int i = 0; i < 32; i++) _regs[i] = new byte[VLenBytes * 8]; // Generous allocation
        }

        public void UpdateVtype(ulong newVtype, int avl)
        {
            Vtype = newVtype;
            
            // SEW: 000=8b, 001=16b, 010=32b, 011=64b
            int sewCode = (int)((newVtype >> 3) & 0x7);
            SewBytes = 1 << sewCode;

            // LMUL
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
            
            int vlmax = (int)((VLen / (SewBytes * 8)) * Lmul);
            if (vlmax < 0) vlmax = 0; 

            Vl = (avl < vlmax) ? avl : vlmax;
        }

        // Renamed back to GetRaw/WriteRaw to fix CS1061 errors in Instruction files
        public byte[] GetRaw(int index) => _regs[index];
        
        public void WriteRaw(int index, byte[] data)
        {
             if(index >= 0 && index < 32) 
                Array.Copy(data, _regs[index], Math.Min(data.Length, _regs[index].Length));
        }

        public void Reset() 
        { 
            for (int i = 0; i < 32; i++) Array.Clear(_regs[i], 0, _regs[i].Length); 
            Vstart = 0; Vl = 0;
        }
    }
}
