.text
main:
    li t0, 1000      # Loop count
    li x5, 5         # Variable 'x'

loop:
    # Calculate: Result = (0x12345678 * x) + 0xABCDEF00

    # 1. Load Constant A (Fused)
    lui x10, 0x12345
    addi x10, x10, 0x678

    # 2. Load Constant B (Fused)
    lui x11, 0xABCDE
    addi x11, x11, 0xF00

    # 3. Math (Real Work)
    # If width=4:
    # Without Fusion: LUI, ADDI, LUI, ADDI fill the bundle. MUL/ADD pushed to next cycle.
    # With Fusion: [LUI+ADDI], [LUI+ADDI], MUL, ADD all fit in ONE bundle.
    mul x12, x10, x5
    add x12, x12, x11

    addi t0, t0, -1
    bnez t0, loop

exit:
    li a7, 10
    ecall