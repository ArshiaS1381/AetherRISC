.data
msg: .asciz "AetherRISC"
len: .word 9

.text
main:
    li t0, -1
    la t1, msg
    la t2, len
    lw t2, 0(t2)

byte_loop:
    beqz t2, finish
    lbu t3, 0(t1)
    xor t0, t0, t3

    li t4, 8
bit_loop:
    andi t5, t0, 1
    srli t0, t0, 1
    beqz t5, no_xor
    li t6, 0xEDB88320
    xor t0, t0, t6
no_xor:
    addi t4, t4, -1
    bnez t4, bit_loop

    addi t1, t1, 1
    addi t2, t2, -1
    j byte_loop

finish:
    not t0, t0
    mv a0, t0
    li a7, 1
    ecall
    li a7, 10
    ecall
