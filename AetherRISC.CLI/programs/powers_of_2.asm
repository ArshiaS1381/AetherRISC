.text
main:
    addi x5, x0, 1       # Result = 1
    addi x6, x0, 13      # Loop 13 times (up to 4096)
    addi x20, x0, 0      # Counter i = 0

loop:
    bge x20, x6, exit

    # Print Number
    mv a0, x5
    li a7, 1
    ecall

    # Print Space
    addi a0, x0, 32
    li a7, 11
    ecall

    # Shift Left (Multiply by 2)
    slli x5, x5, 1
    
    addi x20, x20, 1     # i++
    j loop

exit:
    addi a0, x0, 10      # Newline
    li a7, 11
    ecall
    li a7, 10            # Exit
    ecall
