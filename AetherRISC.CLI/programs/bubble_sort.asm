# Bubble Sort - tests memory load/store, nested loops
.data
array: .word 64, 34, 25, 12, 22, 11, 90, 42
size:  .word 8

.text
main:
    la s0, array
    la t0, size
    lw s1, 0(t0)        # s1 = size
    
    # Bubble sort outer loop
    addi s2, s1, -1     # s2 = n-1 (outer counter)
outer:
    blez s2, print_array
    
    li s3, 0            # s3 = inner counter
    mv s4, s0           # s4 = current pointer
inner:
    addi t0, s2, 0
    bge s3, t0, outer_next
    
    lw t1, 0(s4)        # t1 = arr[j]
    lw t2, 4(s4)        # t2 = arr[j+1]
    
    ble t1, t2, no_swap
    # Swap
    sw t2, 0(s4)
    sw t1, 4(s4)
no_swap:
    addi s4, s4, 4
    addi s3, s3, 1
    j inner

outer_next:
    addi s2, s2, -1
    j outer

print_array:
    mv s2, s0           # pointer
    mv s3, s1           # counter
print_loop:
    beqz s3, done
    lw a0, 0(s2)
    li a7, 1
    ecall
    li a0, 32           # space
    li a7, 11
    ecall
    addi s2, s2, 4
    addi s3, s3, -1
    j print_loop

done:
    li a0, 10
    li a7, 11
    ecall
    li a7, 10
    ecall
