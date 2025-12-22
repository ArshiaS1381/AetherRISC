# AetherRISC
**Version:** 0.0.0 (Pre-Alpha) | **Status:** Active Development (DO NOT USE)

AetherRISC is a high-fidelity, deterministic RISC-V runtime environment and simulation platform designed to supersede legacy educational tools like Venus, RARS, and MARS[cite: 383]. It bridges the gap between pedagogical visualization and industrial systems analysis by combining a cycle-accurate simulation core with a modern, "Interactive Hybrid" web architecture[cite: 383, 385].

> **Note:** This project targets **.NET 10** and **C# 14**[cite: 22]. Ensure your environment is configured for preview features.

---

## System Architecture

AetherRISC implements a **Hybrid Client-Server Architecture** utilizing the .NET Blazor "Interactive Auto" render mode[cite: 6, 7]. This design strictly separates concerns between management and execution:

* **Execution Plane (Client-Side WASM):** Runs the simulation loop on the user's hardware (via WebAssembly or .NET MAUI) to achieve zero-latency execution (>100 KHz) without server compute costs[cite: 15, 19].
* **Management Plane (Server-Side):** Handles authentication, project persistence, and heavy-lifting tasks like the Docker-based Polyglot Compiler[cite: 10, 11].

The codebase follows the **Clean Architecture (Onion Architecture)** pattern, ensuring the `AetherRISC.Core` domain logic has zero dependencies on the UI or external infrastructure[cite: 42, 43].

---

## Key Features

### 1. Cycle-Accurate Simulation
* **5-Stage Pipeline:** Fully visualized Fetch, Decode, Execute, Memory, and Writeback stages with hazard detection and forwarding[cite: 255, 425].
* **Memory Hierarchy:** Configurable L1 (Split I/D) and L2 caches with visualization of hits, misses, and coherence states (Valid/Dirty)[cite: 272, 481].
* **Extensions:** Supports RV32I/RV64I with M, A, F, D, C, and V (Vector) extensions[cite: 116, 122].

### 2. Runtime Intervention Subsystem (RIS)
* **"God Mode" Controls:** Dynamic Code Injection (DCI) via memory trampolining allows you to insert arbitrary opcodes into a running stream[cite: 395, 437].
* **Time Travel:** State Checkpointing using a Memento pattern allows for "rewinding" execution to previous cycles[cite: 317, 443].
* **Direct Register Access (DRA):** Modify register states in real-time with automatic pipeline flushing and dependency re-evaluation[cite: 309, 311].

### 3. Heuristic Diagnostic Engine (HDE)
* **Deterministic Explanations:** Replaces "AI" guessing with static analysis to explain *exactly* why a stall occurred (e.g., "Load-Use Hazard on x5")[cite: 324, 445].
* **Rosetta View:** Real-time projection of Assembly into "Pseudo-C" syntax (e.g., `addi xA, xA, 1` $\rightarrow$ `xA++`)[cite: 327, 475].

### 4. Embedded & Legacy Support
* **Virtual PCB:** Interactive SVG-based circuit board visualization utilizing a custom aesthetic for high-fidelity UI rendering[cite: 355].
* **Legacy Parity:** 100% compatible with Venus and RARS system calls (ecall 1-57), file formats, and project structures[cite: 406, 408].

---

## Technology Stack

### Core Frameworks
* **Language:** C# 14 / .NET 10 Preview [cite: 22]
* **Web:** Blazor Web App (Interactive Auto) [cite: 23]
* **Desktop:** .NET MAUI (wrapping Blazor via WebView2) [cite: 24]

### Critical Libraries
* **System.Runtime.Intrinsics:** SIMD acceleration for RISC-V Vector operations[cite: 31].
* **Google.Protobuf:** High-performance binary serialization for state snapshots[cite: 35].
* **ELFSharp:** Parsing ELF binaries and DWARF debug symbols[cite: 31].
* **Blazor.Extensions.Canvas:** HTML5 Canvas Interop for rendering the pipeline visualizer[cite: 39].