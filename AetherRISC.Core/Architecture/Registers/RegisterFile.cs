namespace AetherRISC.Core.Architecture.Registers;

public class RegisterFile
{
    private readonly ulong[] _registers = new ulong[32];
    private readonly SystemConfig _config;

    public RegisterFile(SystemConfig config = null)
    {
        // Default to RV64 if no config provided for backward compatibility
        _config = config ?? SystemConfig.Rv64();
    }

    public ulong Read(int index)
    {
        if (index == 0) return 0;
        
        // In RV32, even if we store ulong, the upper bits should effectively be ignored/sign-extended.
        // But if we enforce clamping on WRITE, reads are safe.
        return _registers[index];
    }
    
    // Helper for debugging/tests to get signed 32-bit view
    public int Read32(int index) => (int)_registers[index];

    public void Write(int index, ulong value)
    {
        if (index == 0) return;

        if (_config.Architecture == ArchitectureMode.Rv32)
        {
            // --- RV32 MODE: TRUNCATE ---
            // Simulate 32-bit register behavior by masking.
            // When reading back as ulong later, it will look like a zero-extended 32-bit value.
            // (Note: RISC-V usually sign-extends 32-bit values into 64-bit registers, 
            // but for a pure RV32 simulation, zero-extension or simple masking is sufficient 
            // as instructions wont look at upper bits).
            _registers[index] = value & 0xFFFFFFFF;
        }
        else
        {
            // --- RV64 MODE: FULL WIDTH ---
            _registers[index] = value;
        }
    }
}
