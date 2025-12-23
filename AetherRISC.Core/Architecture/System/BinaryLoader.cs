using System;
using System.IO;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.System
{
    public static class BinaryLoader
    {
        public static void Load(MachineState state, string filePath, ulong loadAddress)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Binary file not found: {filePath}");

            byte[] program = File.ReadAllBytes(filePath);
            
            // Inject bytes into the Memory Bus
            for (int i = 0; i < program.Length; i++)
            {
                state.Memory!.WriteByte((uint)(loadAddress + (ulong)i), program[i]);
            }

            // Set the PC to the start of the loaded program
            state.ProgramCounter = loadAddress;
            
            Console.WriteLine($"Loaded {program.Length} bytes at 0x{loadAddress:X8}");
        }
    }
}
