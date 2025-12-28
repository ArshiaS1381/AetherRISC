.text
main:
    fence.i

    li a0, 12345
    li a7, 1
    ecall

    li a0, 10
    li a7, 11
    ecall

    li a7, 10
    ecall
