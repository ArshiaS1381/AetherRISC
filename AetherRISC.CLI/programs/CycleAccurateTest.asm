.text
main:
  # RISC-V Branch Predictor Test
  # Registers: t0 = outer, t1 = inner, t2 = work
  LI t0, 2           # Outer loop runs 2 times
  LI t2, 0           # Initialize work register

outer_loop:
  LI t1, 4           # Inner loop runs 4 times (Sequence: T, T, T, N)

inner_loop:
  ADDI t2, t2, 1     # Do some work
  ADDI t1, t1, -1    # Decrement inner
  BNE t1, zero, inner_loop # Inner Branch (Target)

  ADDI t0, t0, -1    # Decrement outer
  BNE t0, zero, outer_loop # Outer Branch (Target)
  EBREAK             # End Simulation