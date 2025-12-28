.data
    val1: .word 0xDEADBEEF
    val2: .word 0xCAFEBABE
    result: .word 0x00000000

.text
main:
    # 1. Load addresses of data labels (LA pseudo-instruction)
    la x5, val1
    la x6, val2
    
    # 2. Load values from memory
    lw x10, 0(x5)         # Load DEADBEEF into x10
    lw x11, 0(x6)         # Load CAFEBABE into x11
    
    # 3. Perform operation (XOR)
    xor x12, x10, x11     # x12 = DEADBEEF ^ CAFEBABE
    
    # 4. Store result back to memory
    la x7, result
    sw x12, 0(x7)         # Store result
    
    ebreak
