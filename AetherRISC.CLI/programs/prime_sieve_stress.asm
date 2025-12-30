.data
N: .word 100000
flags: .space 100

.text
main:
    la t0, N
    lw s0, 0(t0)
    la s1, flags

    li t0, 2
outer:
    bge t0, s0, print

    sh1add t1, t0, s1
    lbu t2, 0(t1)
    bnez t2, next_i

    mul t3, t0, t0
inner:
    bge t3, s0, next_i
    sh1add t4, t3, s1
    li t5, 1
    sb t5, 0(t4)
    add t3, t3, t0
    j inner

next_i:
    addi t0, t0, 1
    j outer

print:
    li t0, 2
print_loop:
    bge t0, s0, done
    sh1add t1, t0, s1
    lbu t2, 0(t1)
    bnez t2, skip

    mv a0, t0
    li a7, 1
    ecall
    li a0, 32
    li a7, 11
    ecall

    addi x31, x31, 1

skip:
    addi t0, t0, 1
    j print_loop

done:
    li a7, 10
    ecall
