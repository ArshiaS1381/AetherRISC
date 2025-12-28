.text
main:
    addi x5, x0, 105     # a = 105
    addi x6, x0, 252     # b = 252

gcd_loop:
    beq x6, x0, done     # if b == 0, result is a

    # t = b
    mv x7, x6
    
    # b = a % b
    # RISC-V REM instruction (Remainder)
    rem x6, x5, x6
    
    # a = t
    mv x5, x7
    j gcd_loop

done:
    # Print Result
    mv a0, x5
    li a7, 1
    ecall
    
    addi a0, x0, 10      # Newline
    li a7, 11
    ecall
    li a7, 10
    ecall
