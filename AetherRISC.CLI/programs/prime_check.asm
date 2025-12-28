.text
main:
    addi x5, x0, 127     # Number to check
    addi x6, x0, 2       # Divisor i = 2
    
    # Handle simple cases (n < 2)
    addi x7, x0, 2
    blt x5, x7, not_prime

loop:
    mul x8, x6, x6       # t = i * i
    bgt x8, x5, is_prime # If i*i > n, we found no divisors. It's prime.
    
    rem x9, x5, x6       # rem = n % i
    beq x9, x0, not_prime # If rem == 0, it divides evenly. Not prime.
    
    addi x6, x6, 1       # i++
    j loop

is_prime:
    addi a0, x0, 1       # Load 1 (True)
    j print

not_prime:
    addi a0, x0, 0       # Load 0 (False)

print:
    li a7, 1
    ecall
    
    addi a0, x0, 10      # Newline
    li a7, 11
    ecall
    li a7, 10
    ecall
