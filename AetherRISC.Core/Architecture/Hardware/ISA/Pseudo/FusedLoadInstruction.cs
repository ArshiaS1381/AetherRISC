using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo
{
    public class FusedLoadInstruction : IInstruction
    {
        private readonly int _rd;
        private readonly long _targetAddress;
        private readonly int _width; 
        private readonly bool _signed;
        private readonly string _name;

        public string Mnemonic => _name;
        public int Rd => _rd;
        public int Rs1 => 0;
        public int Rs2 => 0;
        public int Imm => 0; 

        public bool IsLoad => true;
        public bool IsStore => false;
        public bool IsBranch => false;
        public bool IsJump => false;
        public bool IsFloatRegWrite => false;

        public FusedLoadInstruction(string name, int rd, long addr, int width, bool signed)
        {
            _name = name;
            _rd = rd;
            _targetAddress = addr;
            _width = width;
            _signed = signed;
        }

        public void Execute(MachineState state, InstructionData data)
        {
            ulong val = ReadMem(state.Memory, (uint)_targetAddress);
            state.Registers.Write(_rd, val);
        }

        public void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineMicroOp op)
        {
            op.AluResult = (ulong)_targetAddress;
        }

        // Fix: Allow nullable memory bus
        private ulong ReadMem(IMemoryBus? mem, uint addr)
        {
            if (mem == null) return 0;
            switch (_width)
            {
                case 1: 
                    byte b = mem.ReadByte(addr); 
                    return _signed ? (ulong)(long)(sbyte)b : (ulong)b;
                case 2:
                    ushort h = mem.ReadHalf(addr);
                    return _signed ? (ulong)(long)(short)h : (ulong)h;
                case 4:
                    uint w = mem.ReadWord(addr);
                    return _signed ? (ulong)(long)(int)w : (ulong)w;
                case 8:
                    return mem.ReadDouble(addr);
            }
            return 0;
        }
    }
}
