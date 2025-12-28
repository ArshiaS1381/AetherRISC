.data
# IEEE-754 single-precision:
# 3.5  = 0x40600000
# 2.25 = 0x40100000
# 4.0  = 0x40800000
f_a: .word 0x40600000
f_b: .word 0x40100000
f_c: .word 0x40800000

.text
main:
    la t0, f_a
    flw f0, t0, 0

    la t0, f_b
    flw f1, t0, 0

    la t0, f_c
    flw f2, t0, 0

    fadd.s f3, f0, f1
    fmul.s f4, f3, f2

    fcvt.w.s a0, f4
    li a7, 1
    ecall

    li a0, 10
    li a7, 11
    ecall

    li a7, 10
    ecall
