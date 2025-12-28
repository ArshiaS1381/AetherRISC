.text
main:
    # Write mstatus = 0x8, get old value in a0
    li t0, 0x8
    csrrw a0, mstatus, t0
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Read mstatus into a0 (using csrrs with x0)
    csrrs a0, mstatus, x0
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Set bits 0x3 in mstatus, return old in a0
    li t0, 0x3
    csrrs a0, mstatus, t0
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Clear bit 0x1 in mstatus, return old in a0
    li t0, 0x1
    csrrc a0, mstatus, t0
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    # Final read
    csrrs a0, mstatus, x0
    li a7, 1
    ecall
    li a0, 10
    li a7, 11
    ecall

    li a7, 10
    ecall
