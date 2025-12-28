.text
# Calculate and print 10 Fibonacci numbers
main:
    addi x5, x0, 10      # Count = 10
    addi x6, x0, 0       # a = 0
    addi x7, x0, 1       # b = 1
    addi x20, x0, 0      # i = 0

fib_loop:
    bge x20, x5, done    # if i >= count, exit

    # --- Print Number ---
    mv a0, x6            # Move 'a' to a0
    li a7, 1             # Print Int
    ecall

    # --- Print Space (ASCII 32) ---
    addi a0, x0, 32      # Load Space character
    li a7, 11            # Syscall 11 = Print Char
    ecall
    
    # --- Calculate Next ---
    add x8, x6, x7       # temp = a + b
    mv x6, x7            # a = b
    mv x7, x8            # b = temp
    
    addi x20, x20, 1     # i++
    j fib_loop

done:
    # Print Newline (Optional)
    addi a0, x0, 10
    li a7, 11
    ecall

    li a7, 10            # Exit
    ecall
