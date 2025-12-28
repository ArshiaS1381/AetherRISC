.data
# IEEE-754 double (little-endian in memory):
# 1.5 = 0x3FF8000000000000 (lo=0x00000000 hi=0x3FF80000)
d_a_lo: .word 0x00000000
d_a_hi: .word 0x3FF80000

# 2.0 = 0x4000000000000000 (lo=0x00000000 hi=0x40000000)
d_b_lo: .word 0x00000000
d_b_hi: .word 0x40000000

# 0.5 = 0x3FE0000000000000 (lo=0x00000000 hi=0x3FE00000)
d_c_lo: .word 0x00000000
d_c_hi: .word 0x3FE00000

.text
main:
    la t0, d_a_lo
    fld f0, t0, 0

    la t0, d_b_lo
    fld f1, t0, 0

    la t0, d_c_lo
    fld f2, t0, 0

    fadd.d f3, f0, f1
    fdiv.d f4, f3, f2

    fcvt.w.d a0, f4
    li a7, 1
    ecall

    li a0, 10
    li a7, 11
    ecall

    li a7, 10
    ecall
