.text
main:
    addi x5, x0, 100     # Limit N = 100
    addi x6, x0, 0       # Sum = 0
    addi x7, x0, 1       # Counter = 1

loop:
    bgt x7, x5, print    # If counter > N, print

    add x6, x6, x7       # Sum += Counter
    addi x7, x7, 1       # Counter++
    j loop

print:
    mv a0, x6
    li a7, 1
    ecall
    
    addi a0, x0, 10      # Newline
    li a7, 11
    ecall
    li a7, 10
    ecall
