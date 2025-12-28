using System;
using System.Text;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.CLI
{
    public class SimpleHost : IHostSystem
    {
        public void HandleBreak(MachineState state)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[HOST] Breakpoint (EBREAK) hit. Halting CPU.");
            Console.ResetColor();
            state.Halted = true;
        }

        public void HandleEcall(MachineState state)
        {
            // RARS/Mars Convention
            // a7 (x17) = System Call ID
            // a0 (x10) = Argument
            
            long syscall = (long)state.Registers.Read(17); 
            ulong arg0   = state.Registers.Read(10);    

            switch (syscall)
            {
                case 1: // Print Integer
                    Console.Write((long)arg0); 
                    break;

                case 4: // Print String
                    PrintString(state, (uint)arg0);
                    break;
                
                case 11: // Print Character
                    Console.Write((char)arg0);
                    break;

                case 10: // Exit
                case 93: // Exit (Linux convention)
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[HOST] Program exited successfully (ECALL).");
                    Console.ResetColor();
                    state.Halted = true;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[HOST] Unknown Syscall ID: {syscall}");
                    Console.ResetColor();
                    state.Halted = true;
                    break;
            }
        }

        private void PrintString(MachineState state, uint address)
        {
            if (state.Memory == null) return;

            // Read bytes until null terminator (0)
            while (true)
            {
                byte b = state.Memory.ReadByte(address);
                if (b == 0) break;
                Console.Write((char)b);
                address++;
            }
        }
    }
}
