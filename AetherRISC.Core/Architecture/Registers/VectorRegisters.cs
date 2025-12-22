namespace AetherRISC.Core.Architecture.Registers;

public class VectorRegisters
{
    // 32 vector registers.
    // Each register is a byte array of length VLEN (in bytes).
    private readonly byte[][] _registers;
    public int VLenBytes { get; }

    // Vector Control Registers
    public ulong Vstart { get; set; }
    public ulong Vxsat { get; set; }
    public ulong Vxrm { get; set; }
    public ulong Vcsr { get; set; }
    public ulong Vtype { get; set; }
    public ulong Vlenb { get; private set; } // VLEN in bytes

    public VectorRegisters(int vlenBits = 128)
    {
        VLenBytes = vlenBits / 8;
        Vlenb = (ulong)VLenBytes;
        
        _registers = new byte[32][];
        for (int i = 0; i < 32; i++)
        {
            _registers[i] = new byte[VLenBytes];
        }
    }

    public byte[] Read(int index)
    {
        if (index < 0 || index >= 32) return Array.Empty<byte>();
        return _registers[index];
    }

    public void Write(int index, byte[] data)
    {
        if (index < 0 || index >= 32) return;
        if (data.Length != VLenBytes) return; // Strict width check

        Array.Copy(data, _registers[index], VLenBytes);
    }
    
    public void Reset()
    {
        for (int i = 0; i < 32; i++) Array.Clear(_registers[i], 0, VLenBytes);
        Vstart = 0;
    }
}
