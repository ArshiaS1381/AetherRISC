.text
main:
    addi x5, x0, 7       # Base = 7
    addi x6, x0, 1       # i = 1
    addi x7, x0, 11      # Limit = 11

loop:
    bge x6, x7, exit
    
    mul x8, x5, x6       # res = 7 * i
    
    # Print
    mv a0, x8
    li a7, 1
    ecall
    
    # Space
    addi a0, x0, 32
    li a7, 11
    ecall
    
    addi x6, x6, 1
    j loop

exit:
    addi a0, x0, 10      # Newline
    li a7, 11
    ecall
    li a7, 10
    ecall
