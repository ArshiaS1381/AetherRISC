# GCD using Euclidean Algorithm - tests branches, REM
.text
main:
    li a0, 48
    li a1, 18
    jal ra, gcd
    
    li a7, 1
    ecall
    
    li a0, 10
    li a7, 11
    ecall
    
    li a7, 10
    ecall

gcd:
    # gcd(a, b): while b != 0: a, b = b, a % b
gcd_loop:
    beqz a1, gcd_done
    rem t0, a0, a1      # t0 = a % b
    mv a0, a1           # a = b
    mv a1, t0           # b = a % b
    j gcd_loop

gcd_done:
    ret
