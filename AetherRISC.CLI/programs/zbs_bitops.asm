.text
main:
    li t0, 0

    # Set bit 5 => 32
    bseti t1, t0, 5
    mv a0, t1
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Extract bit 5 => 1
    bexti t2, t1, 5
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Invert bit 5 => back to 0
    binvi t3, t1, 5
    mv a0, t3
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Clear bit 5 (already 0, stays 0)
    bclri t4, t3, 5
    mv a0, t4
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Register-based forms:
    li t5, 7
    bset t6, t0, t5         # set bit 7 => 128
    mv a0, t6
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    bext t1, t6, t5         # extract bit 7 => 1
    mv a0, t1
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    li a7, 10
    ecall
