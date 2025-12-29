# Interactive Sieve of Eratosthenes
# - Asks user for N
# - Calculates primes up to N
# - Prints them out

.data
prompt:     .asciz "Enter upper limit (max 10000): "
result_msg: .asciz "\nPrimes found:\n"
buffer:     .space 10001        # Reserve 10k bytes. 0 = Prime, 1 = Composite

.text
.globl main
main:
    # 1. Print Prompt
    la a0, prompt       # Load address of prompt
    li a7, 4            # Syscall 4: Print String
    ecall

    # 2. Read Input (N)
    li a7, 5            # Syscall 5: Read Integer
    ecall
    mv s0, a0           # Save N into s0

    # 3. Sanity Check / Cap Input
    # If input < 2, exit
    li t0, 2
    blt s0, t0, exit
    
    # If input > 10000, cap at 10000 (buffer size)
    li t0, 10000
    ble s0, t0, init_sieve
    mv s0, t0           # N = 10000

init_sieve:
    la s1, buffer       # s1 = Base address of flags array

    # 4. Sieve Algorithm
    # Loop i from 2 to sqrt(N)
    li t0, 2            # i = 2

outer_loop:
    mul t1, t0, t0      # t1 = i * i
    bgt t1, s0, print_header  # If i*i > N, we are done marking

    # Check if flags[i] is already marked
    add t2, s1, t0      # addr = base + i
    lb t3, 0(t2)        # load byte
    bnez t3, next_iter  # If not 0 (composite), skip

    # Inner Loop: Mark multiples of i starting at i*i
    # t1 is currently i*i
inner_loop:
    bgt t1, s0, next_iter     # If j > N, stop inner loop
    
    add t2, s1, t1      # addr = base + j
    li t3, 1
    sb t3, 0(t2)        # flags[j] = 1 (Composite)
    
    add t1, t1, t0      # j = j + i
    j inner_loop

next_iter:
    addi t0, t0, 1      # i++
    j outer_loop

    # 5. Print Results
print_header:
    la a0, result_msg
    li a7, 4
    ecall

    li t0, 2            # i = 2 (Reset counter)

print_loop:
    bgt t0, s0, exit    # If i > N, done

    add t1, s1, t0      # addr = base + i
    lb t2, 0(t1)        # load flags[i]
    bnez t2, print_next # If composite, skip print

    # Print Prime Number
    mv a0, t0
    li a7, 1            # Print Int
    ecall

    # Print Space
    li a0, 32           # Space char
    li a7, 11           # Print Char
    ecall

print_next:
    addi t0, t0, 1
    j print_loop

exit:
    # Print Newline
    li a0, 10
    li a7, 11
    ecall
    
    # Exit
    li a7, 10
    ecall