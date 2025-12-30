.text
main:
    li t0, 1000        # Run loop 1000 times
    li t1, 0           # Counter

loop:
    # Pair 1: Load 0x12345678
    lui x1, 0x12345
    addi x1, x1, 0x678

    # Pair 2: Load 0x87654321
    lui x2, 0x87654
    addi x2, x2, 0x321

    # Pair 3: Load 0xAABBCCDD
    lui x3, 0xAABBC
    addi x3, x3, 0xCDD

    # Pair 4: Load 0x11223344
    lui x4, 0x11223
    addi x4, x4, 0x344

    # Loop Logic
    addi t1, t1, 1
    blt t1, t0, loop

exit:
    li a7, 10
    ecall