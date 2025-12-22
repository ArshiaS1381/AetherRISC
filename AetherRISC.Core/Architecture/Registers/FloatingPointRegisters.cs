namespace AetherRISC.Core.Architecture.Registers;

public class FloatingPointRegisters
{
    // 32 registers, 64-bit width (to hold Doubles).
    private readonly double[] _registers = new double[32];

    // Control Status Register for rounding modes and flags (fcsr)
    public uint Fcsr { get; set; }

    public double ReadDouble(int index)
    {
        if (index < 0 || index >= 32) return 0.0;
        return _registers[index];
    }

    public float ReadSingle(int index)
    {
        // RISC-V F extension behavior: check for NaN boxing here in valid hardware,
        // but for now we simply cast down.
        return (float)_registers[index];
    }

    public void WriteDouble(int index, double value)
    {
        if (index >= 0 && index < 32)
        {
            _registers[index] = value;
        }
    }

    public void WriteSingle(int index, float value)
    {
        if (index >= 0 && index < 32)
        {
            // When writing a single float into a 64-bit register, 
            // RISC-V requires "NaN Boxing" (setting upper bits to 1).
            // We store the simple value for now, but implementation detail goes here.
            _registers[index] = (double)value;
        }
    }

    public void Reset()
    {
        Array.Clear(_registers, 0, 32);
        Fcsr = 0;
    }
}
