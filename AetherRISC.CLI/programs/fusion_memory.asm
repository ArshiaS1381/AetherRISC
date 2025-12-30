.data
# Create an array of data at a known high address (conceptually)
val1: .word 10
val2: .word 20
val3: .word 30
val4: .word 40

.text
main:
    li t0, 1000     # Iterations
    li t1, 0

loop:
    # In standard RISC-V, accessing globals is often:
    # LUI reg, %hi(addr)
    # LW  reg, %lo(addr)(reg)
    
    # We manually construct this pattern to ensure the assembler 
    # doesn't optimize it into a GP-relative load.
    
    # 1. Access val1 (Base + Offset 0) -> Fused to FusedLoadInstruction
    lui x5, 0x10010      # High 20 bits of .data (usually 0x10010000)
    lw  x10, 0(x5)       # Low 12 bits (0)

    # 2. Access val2 (Base + Offset 4) -> Fused to FusedLoadInstruction
    lui x6, 0x10010
    lw  x11, 4(x6)

    # 3. Access val3 (Base + Offset 8) -> Fused to FusedLoadInstruction
    lui x7, 0x10010
    lw  x12, 8(x7)
    
    # 4. Access val4 (Base + Offset 12) -> Fused to FusedLoadInstruction
    lui x8, 0x10010
    lw  x13, 12(x8)

    addi t1, t1, 1
    blt t1, t0, loop

exit:
    li a7, 10
    ecall