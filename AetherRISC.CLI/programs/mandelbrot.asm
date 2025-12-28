.data
width: .word 40
height: .word 20

.text
main:
    la t0, width
    lw s0, 0(t0)
    la t0, height
    lw s1, 0(t0)

    li t0, 0
row:
    bge t0, s1, done
    li t1, 0
col:
    bge t1, s0, newline
    li a0, 35
    li a7, 11
    ecall
    addi t1, t1, 1
    j col

newline:
    li a0, 10
    li a7, 11
    ecall
    addi t0, t0, 1
    j row

done:
    li a7, 10
    ecall
