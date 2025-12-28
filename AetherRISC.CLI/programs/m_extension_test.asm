# M-Extension Test: MUL, DIV, REM
.text
main:
    # Test MUL: 7 * 6 = 42
    li t0, 7
    li t1, 6
    mul t2, t0, t1
    mv a0, t2
    li a7, 1
    ecall
    
    li a0, 10       # newline
    li a7, 11
    ecall
    
    # Test DIV: 42 / 6 = 7
    li t0, 42
    li t1, 6
    div t2, t0, t1
    mv a0, t2
    li a7, 1
    ecall
    
    li a0, 10
    li a7, 11
    ecall
    
    # Test REM: 43 % 6 = 1
    li t0, 43
    li t1, 6
    rem t2, t0, t1
    mv a0, t2
    li a7, 1
    ecall
    
    li a0, 10
    li a7, 11
    ecall
    
    # Test DIVU (unsigned): large positive / 2
    li t0, 0xFFFFFFFF
    li t1, 2
    divu t2, t0, t1
    
    # Test negative division: -10 / 3 = -3 (truncate toward zero)
    li t0, -10
    li t1, 3
    div t2, t0, t1
    mv a0, t2
    li a7, 1
    ecall
    
    li a0, 10
    li a7, 11
    ecall
    
    li a7, 10
    ecall
