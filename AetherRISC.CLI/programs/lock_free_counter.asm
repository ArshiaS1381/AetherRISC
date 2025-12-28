.data
counter: .word 0

.text
main:
    la t0, counter

retry:
    lr.w t1, 0(t0)
    addi t1, t1, 1
    sc.w t2, t1, 0(t0)
    bnez t2, retry

    lw a0, 0(t0)
    li a7, 1
    ecall
    li a7, 10
    ecall