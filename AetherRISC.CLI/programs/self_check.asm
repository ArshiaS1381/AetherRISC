.text
main:
    li t0, 5
    li t1, 10
    add t2, t0, t1
    li t3, 15
    bne t2, t3, fail

    li a0, 80
    li a7, 11
    ecall
    li a0, 65
    li a7, 11
    ecall
    li a0, 83
    li a7, 11
    ecall
    li a0, 83
    li a7, 11
    ecall
    j done

fail:
    li a0, 70
    li a7, 11
    ecall

done:
    li a7, 10
    ecall
