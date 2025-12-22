namespace AetherRISC.Core.Abstractions.Interfaces;

public interface ISystemCallHandler
{
    void PrintInt(long value);
    void PrintString(string value);
    void Exit(int code);
    // Add more (Open, Read, Write) later
}
