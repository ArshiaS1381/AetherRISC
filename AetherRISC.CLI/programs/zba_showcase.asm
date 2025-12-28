.text
main:
    # ---- Zba: ADD.UW ----
    li t0, -1               # 0xFFFF...FFFF
    li t1, 5
    add.uw t2, t0, t1       # t2 = zero_extend((uint)t0) + t1 = 0xFFFFFFFF + 5
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # ---- Zba: SH1ADD/SH2ADD/SH3ADD ----
    li t0, 7
    li t1, 3
    sh1add t2, t0, t1       # t2 = (t0<<1) + t1 = 14 + 3 = 17
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    sh2add t2, t0, t1       # (t0<<2)+t1 = 28+3=31
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    sh3add t2, t0, t1       # (t0<<3)+t1 = 56+3=59
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # ---- Zba: SLLI.UW ----
    li t0, -1
    slli.uw t2, t0, 8       # zero_extend((uint)t0) << 8  => 0xFFFFFFFF00
    mv a0, t2
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    li a7, 10
    ecall
