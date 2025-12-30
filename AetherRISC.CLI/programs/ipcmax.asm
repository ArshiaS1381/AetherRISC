.text
main:
    # Use 'li' pseudo-op for safety
    li x1, 1
    li x2, 1
    li x3, 1
    li x4, 1
    li x5, 1
    li x6, 1
    li x7, 1
    li x8, 1
    li x9, 1
    li x10, 1
    li x11, 1
    li x12, 1
    li x13, 1
    li x14, 1
    li x15, 1
    li x16, 1

    # Counter: 100 iterations
    li x31, 100 

# NO ALIGN HERE - Just the label
ipc_burst:
    addi x1, x1, 1
    addi x2, x2, 1
    addi x3, x3, 1
    addi x4, x4, 1
    addi x5, x5, 1
    addi x6, x6, 1
    addi x7, x7, 1
    addi x8, x8, 1
    addi x9, x9, 1
    addi x10, x10, 1
    addi x11, x11, 1
    addi x12, x12, 1
    addi x13, x13, 1
    addi x14, x14, 1
    addi x15, x15, 1
    addi x16, x16, 1

    # Decrement
    addi x31, x31, -1
    
    # Branch back if x31 > 0
    # Note: Using BNE with x0 is the most standard way
    bne x31, x0, ipc_burst

done:
    li a7, 10
    ecall