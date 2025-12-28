.text
main:
    addi x5, x0, 27      # Start n = 27
    addi x6, x0, 1       # Constant 1

loop:
    # Print n
    mv a0, x5
    li a7, 1
    ecall

    # Print Arrow "->" (Using Space for simplicity)
    addi a0, x0, 32
    li a7, 11
    ecall

    # Check if n == 1
    beq x5, x6, exit

    # Check if Even (n & 1 == 0)
    andi x7, x5, 1
    beq x7, x0, is_even

is_odd:
    # n = 3n + 1
    addi x8, x0, 3
    mul x5, x5, x8
    addi x5, x5, 1
    j loop

is_even:
    # n = n / 2 (Right Shift)
    srli x5, x5, 1
    j loop

exit:
    addi a0, x0, 10      # Newline
    li a7, 11
    ecall
    li a7, 10
    ecall
