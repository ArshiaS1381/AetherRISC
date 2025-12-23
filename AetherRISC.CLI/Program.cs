/*
 * Project:     AetherRISC
 * File:        Program.cs
 * Version:     2.2.0
 * Description: CLI Entry Point. Adds Architecture Switching (RV32/RV64).
 */

using System;
using System.IO;
using System.Collections.Generic;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using AetherRISC.CLI;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Abstractions.Interfaces;

// --- MENU 1: ARCHITECTURE ---
Console.Clear();
Console.WriteLine("=== AetherRISC System Configuration ===");
Console.WriteLine("1. RV64 (64-bit Mode) - Default");
Console.WriteLine("2. RV32 (32-bit Mode)");
Console.Write("Select Architecture [1-2]: ");
var archChoice = Console.ReadLine();

bool isRv32 = (archChoice == "2");
var config = isRv32 ? SystemConfig.Rv32() : SystemConfig.Rv64();
string archName = isRv32 ? "RV32" : "RV64";

var state = new MachineState(config);
state.Memory = new SystemBus(4096); 
var pipeline = new PipelineController(state);

// --- MENU 2: PROGRAM ---
Console.Clear();
Console.WriteLine($"=== AetherRISC Test Suite ({archName} Mode) ===");
Console.WriteLine("1. Recursive Factorial (5!)");
Console.WriteLine("2. Fibonacci Sequence (First 10)");
Console.WriteLine("3. GCD Algorithm (105, 252)");
Console.WriteLine("4. Array Summation (5 Elements)");
Console.WriteLine("5. Architecture Test (Overflow Check)");
Console.Write("Select Program [1-5]: ");
var choice = Console.ReadLine();

var asm = new LabelAssembler();
asm.Add(pc => Inst.Jal(0, asm.To("entry", pc)));

string algoName = "Unknown";

switch (choice)
{
    case "2":
        algoName = "Fibonacci";
        Console.WriteLine($"Loading {algoName}...");
        asm.Add(pc => Inst.Nop(), label: "entry");
        DemoLibrary.LoadFibonacci(asm, 10);
        break;
    case "3":
        algoName = "GCD";
        Console.WriteLine($"Loading {algoName}...");
        asm.Add(pc => Inst.Nop(), label: "entry");
        DemoLibrary.LoadGCD(asm, 105, 252);
        break;
    case "4":
        algoName = "ArraySum";
        Console.WriteLine($"Loading {algoName}...");
        asm.Add(pc => Inst.Nop(), label: "entry");
        int baseAddr = 1024;
        
        if (isRv32) {
            // RV32: Write 32-bit Words (4 bytes)
            state.Memory.WriteWord((uint)baseAddr, 10);
            state.Memory.WriteWord((uint)baseAddr + 4, 20);
            state.Memory.WriteWord((uint)baseAddr + 8, 30);
            state.Memory.WriteWord((uint)baseAddr + 12, 40);
            state.Memory.WriteWord((uint)baseAddr + 16, 50);
            DemoLibrary.LoadArraySum32(asm, baseAddr, 5); 
        } else {
            // RV64: Write 64-bit DoubleWords (8 bytes)
            state.Memory.WriteDoubleWord((uint)baseAddr, 10);
            state.Memory.WriteDoubleWord((uint)baseAddr + 8, 20);
            state.Memory.WriteDoubleWord((uint)baseAddr + 16, 30);
            state.Memory.WriteDoubleWord((uint)baseAddr + 24, 40);
            state.Memory.WriteDoubleWord((uint)baseAddr + 32, 50);
            DemoLibrary.LoadArraySum64(asm, baseAddr, 5);
        }
        break;
    case "5":
        algoName = "OverflowTest";
        Console.WriteLine($"Loading {algoName}...");
        asm.Add(pc => Inst.Nop(), label: "entry");
        DemoLibrary.LoadOverflowTest(asm);
        break;
    default:
        algoName = "Factorial";
        Console.WriteLine($"Loading {algoName}...");
        asm.Add(pc => Inst.Nop(), label: "entry");
        DemoLibrary.LoadFactorial(asm, 5);
        break;
}

// Exit Block
asm.Add(pc => Inst.Addi(17, 0, 10)); 
asm.Add(pc => Inst.Ecall());

// Assemble
var insts = asm.Assemble();
for(int i=0; i < insts.Count; i++) 
    state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

// --- RUN LOOP ---
string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
string tracePath = $"trace_{archName}_{algoName}_{timestamp}.log";

using (StreamWriter writer = new StreamWriter(tracePath))
{
    Console.WriteLine($"\nTracing to {Path.GetFullPath(tracePath)}...");
    
    // Attach the Logging Host
    state.Host = new FileLoggingHost(writer);

    long cycle = 0;
    bool autoRun = false;
    bool isRunning = true; 

    while (isRunning)
    {
        // Sync running state from host
        if (state.Host is FileLoggingHost loggingHost && !loggingHost.IsRunning) isRunning = false;
        if (!isRunning) break;

        LogState(writer, pipeline, state, cycle);

        if (!autoRun || cycle % 5 == 0)
            Visualizer.RenderPipeline(pipeline, state, cycle);
        
        if (!autoRun)
        {
            var keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Q) break;
            if (keyInfo.Key == ConsoleKey.A) autoRun = true;
        }
        else
        {
            if (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Q) break;
                if (keyInfo.Key == ConsoleKey.S) autoRun = false;
            }
            System.Threading.Thread.Sleep(20); 
        }

        pipeline.Cycle();
        cycle++;
        
        if (cycle > 5000) { 
            Console.WriteLine("Runaway Simulation Detected!"); 
            writer.WriteLine("[ERROR] Runaway Simulation.");
            break; 
        }
    }
}

Console.WriteLine("Simulation Halted.");

void LogState(StreamWriter w, PipelineController p, MachineState s, long c)
{
    w.WriteLine($"--- CYCLE {c} ---");
    w.WriteLine($"IF : PC={p.IfId.PC:X4} | Inst={p.IfId.Instruction:X8}");
    w.WriteLine($"ID : Inst={(p.IdEx.DecodedInst?.Mnemonic ?? "BUBBLE")} | Rd={p.IdEx.Rd}");
    w.WriteLine($"EX : Inst={(p.ExMem.DecodedInst?.Mnemonic ?? "BUBBLE")} | AluRes={p.ExMem.AluResult:X}");
    w.WriteLine($"MEM: Inst={(p.MemWb.DecodedInst?.Mnemonic ?? "BUBBLE")} | Final={p.MemWb.FinalResult:X}");
    w.WriteLine($"WB : RegWrite={p.MemWb.RegWrite}");
    w.WriteLine("REGS:");
    for (int i = 0; i < 32; i += 8)
    {
        w.Write($"  x{i:D2}-x{i+7:D2}: ");
        for(int j=0; j<8; j++) w.Write($"{s.Registers.Read(i+j):X} ");
        w.WriteLine();
    }
    w.WriteLine("");
    w.Flush();
}

