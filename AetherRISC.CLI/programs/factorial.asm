.text
main:
    # 1. Calculate Factorial (5!)
    addi x5, x0, 1       # Result = 1
    addi x6, x0, 5       # Counter = 5
    addi x8, x0, 5       # Save original N for printing

loop:
    beq x6, x0, print    # If counter == 0, go to print
    mul x5, x5, x6       # Result *= Counter
    addi x6, x6, -1      # Counter--
    j loop

print:
    # --- Print "5" ---
    mv a0, x8            # Move N (5) to a0
    li a7, 1             # Syscall 1 = Print Int
    ecall

    # --- Print "!" (ASCII 33) ---
    addi a0, x0, 33
    li a7, 11            # Syscall 11 = Print Char
    ecall

    # --- Print " " (ASCII 32) ---
    addi a0, x0, 32
    li a7, 11
    ecall

    # --- Print "=" (ASCII 61) ---
    addi a0, x0, 61
    li a7, 11
    ecall

    # --- Print " " (ASCII 32) ---
    addi a0, x0, 32
    li a7, 11
    ecall

    # --- Print Result (120) ---
    mv a0, x5            # Move Result to a0
    li a7, 1             # Syscall 1 = Print Int
    ecall

    # --- Print Newline (ASCII 10) ---
    addi a0, x0, 10
    li a7, 11
    ecall

    # --- Exit ---
    li a7, 10            # Syscall 10 = Exit
    ecall
