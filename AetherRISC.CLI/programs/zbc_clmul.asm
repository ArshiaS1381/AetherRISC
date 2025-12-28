.text
main:
    li t0, 0x1234
    li t1, 0x00F0

    clmul  t2, t0, t1
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    clmulr t2, t0, t1
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    clmulh t2, t0, t1
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    li a7, 10
    ecall
