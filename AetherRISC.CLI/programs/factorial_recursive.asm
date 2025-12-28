# Recursive Factorial - tests JAL, JALR, stack operations
.text
main:
    li a0, 10           # Calculate 10!
    jal ra, factorial
    
    # Print result
    li a7, 1
    ecall
    
    li a0, 10           # newline
    li a7, 11
    ecall
    
    li a7, 10           # exit
    ecall

factorial:
    # Base case: if n <= 1, return 1
    li t0, 1
    ble a0, t0, fact_base
    
    # Save ra and n on stack
    addi sp, sp, -16
    sd ra, 8(sp)
    sd a0, 0(sp)
    
    # Recursive call: factorial(n-1)
    addi a0, a0, -1
    jal ra, factorial
    
    # Restore n, multiply: n * factorial(n-1)
    ld t0, 0(sp)
    mul a0, a0, t0
    
    # Restore ra
    ld ra, 8(sp)
    addi sp, sp, 16
    ret

fact_base:
    li a0, 1
    ret
