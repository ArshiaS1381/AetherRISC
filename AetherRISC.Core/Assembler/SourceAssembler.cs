using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Pseudo;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Assembler.Directives;

namespace AetherRISC.Core.Assembler
{
    public enum SectionType { Text, Data, RoData }

    public class SourceAssembler
    {
        private readonly Dictionary<string, uint> _symbolTable = new();
        private readonly string[] _lines;
        private readonly MethodInfo[] _instMethods;
        private readonly List<IAssemblerDirective> _directives = new();

        public uint TextBase { get; set; } = 0x00400000;
        public uint DataBase { get; set; } = 0x10010000;

        public uint CurrentTextPtr { get; private set; }
        public uint CurrentDataPtr { get; private set; }
        public bool InTextSection { get; private set; }
        public bool IsFirstPass { get; private set; }
        public MachineState? Machine { get; private set; }

        public SourceAssembler(string source)
        {
            _lines = source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Select(l => l.Trim()).ToArray();
            _instMethods = typeof(AetherRISC.Core.Architecture.Hardware.ISA.Inst).GetMethods(BindingFlags.Public | BindingFlags.Static);
            RegisterDirectives();
        }

        private void RegisterDirectives()
        {
            _directives.Add(new TextDirective());
            _directives.Add(new DataDirective());
            _directives.Add(new RoDataDirective());
            _directives.Add(new ByteDirective());
            _directives.Add(new HalfDirective());
            _directives.Add(new WordDirective());
            _directives.Add(new AscizDirective());
            _directives.Add(new SpaceDirective());
            _directives.Add(new AlignDirective());
            _directives.Add(new GlobalDirective());
            _directives.Add(new EquDirective());
            _directives.Add(new DwordDirective());
            _directives.Add(new ZeroDirective());
            _directives.Add(new BssDirective());
        }

        public void Assemble(MachineState state)
        {
            Machine = state;
            CurrentTextPtr = TextBase;
            CurrentDataPtr = DataBase;
            InTextSection = true;

            // --- FIRST PASS: SYMBOL RESOLUTION ---
            IsFirstPass = true;
            foreach (var rawLine in _lines)
            {
                var line = StripComments(rawLine);
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.Contains(":")) {
                    var parts = line.Split(':');
                    string label = parts[0].Trim();
                    if (!string.IsNullOrEmpty(label)) _symbolTable[label] = InTextSection ? CurrentTextPtr : CurrentDataPtr;
                    line = parts.Last().Trim();
                }
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                if (line.StartsWith(".")) 
                {
                    HandleDirective(line);
                }
                else if (InTextSection) 
                {
                    // Calculate precise size for pseudos and instructions
                    var (mUpper, args) = ParseLine(line);
                    if (mUpper == null) continue;

                    var expanded = TryExpandPseudo(mUpper, args, CurrentTextPtr);
                    if (expanded != null)
                    {
                        CurrentTextPtr += (uint)(expanded.Count() * 4);
                    }
                    else
                    {
                        // Assume standard 4-byte instruction
                        CurrentTextPtr += 4u;
                    }
                }
            }

            // --- SECOND PASS: CODE GENERATION ---
            IsFirstPass = false;
            CurrentTextPtr = TextBase;
            CurrentDataPtr = DataBase;
            InTextSection = true;
            state.ProgramCounter = TextBase;

            foreach (var rawLine in _lines)
            {
                var line = StripComments(rawLine);
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.Contains(":")) line = line.Split(':').Last().Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith(".")) { HandleDirective(line); continue; }

                var (mUpper, args) = ParseLine(line);
                if (mUpper == null) continue;

                var expanded = TryExpandPseudo(mUpper, args, CurrentTextPtr);
                var toEncode = expanded?.ToList();

                if (toEncode == null)
                {
                    var tokens = Tokenize(line);
                    var mOriginal = tokens[0];

                    if (mUpper == "CSRR") { 
                        mOriginal = "Csrrs";
                        args = new[] { args[0], args[1], "x0" };
                    }
                    else if (mUpper == "CSRW") {
                        mOriginal = "Csrrw";
                        args = new[] { "x0", args[0], args[1] };
                    }
                    
                    toEncode = new List<IInstruction> { BindInstruction(mOriginal, args, CurrentTextPtr) };
                }

                foreach (var inst in toEncode) {
                    uint encoded = InstructionEncoder.Encode(inst);
                    state.Memory!.WriteWord(CurrentTextPtr, encoded);
                    CurrentTextPtr += 4;
                }
            }
        }

        private (string? Mnemonic, string[] Args) ParseLine(string line)
        {
            var tokens = Tokenize(line);
            if (tokens.Length == 0) return (null, Array.Empty<string>());

            var mRaw = tokens[0];
            var mUpper = mRaw.ToUpperInvariant();
            var args = tokens.Skip(1).ToArray();

            // Handle offsets like 0(x1)
            // Note: Tokenizer splits 0(x1) into "0", "x1" automatically usually,
            // but if not, this manual split ensures consistent 3-arg format for Loads/Stores
            if (args.Length == 2 && args[1].Contains("(")) {
                var parts = args[1].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                args = new[] { args[0], parts[0], parts[1] }; 
            }
            return (mUpper, args);
        }

        private void HandleDirective(string line) {
            var tokens = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return;
            var directive = _directives.FirstOrDefault(d => d.Match(tokens[0]));
            if (directive != null) directive.Execute(this, tokens.Skip(1).ToArray());
        }

        public void SwitchSection(SectionType section) => InTextSection = (section == SectionType.Text);

        public void Align(int power) {
            uint align = (uint)Math.Pow(2, power);
            if (InTextSection) CurrentTextPtr = (CurrentTextPtr + align - 1) & ~(align - 1);
            else CurrentDataPtr = (CurrentDataPtr + align - 1) & ~(align - 1);
        }

        public void WriteByte(byte b) {
            if (IsFirstPass) { CurrentDataPtr += 1; return; }
            Machine?.Memory?.WriteByte(CurrentDataPtr++, b);
        }

        public void WriteHalf(ushort h) {
            if (IsFirstPass) { CurrentDataPtr += 2; return; }
            Machine?.Memory?.WriteHalf(CurrentDataPtr, h);
            CurrentDataPtr += 2;
        }

        public void WriteWord(uint w) {
            if (InTextSection) {
                 CurrentTextPtr = (CurrentTextPtr + 3) & ~3u;
                 if (!IsFirstPass) Machine?.Memory?.WriteWord(CurrentTextPtr, w);
                 CurrentTextPtr += 4;
            } else {
                 CurrentDataPtr = (CurrentDataPtr + 3) & ~3u;
                 if (!IsFirstPass) Machine?.Memory?.WriteWord(CurrentDataPtr, w);
                 CurrentDataPtr += 4;
            }
        }
        
        public bool TryParseNumber(string s, out long result) {
            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return long.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out result);
            return long.TryParse(s, out result);
        }

        public uint ResolveSymbol(string s) => _symbolTable.TryGetValue(s, out uint val) ? val : 0;
        public void DefineConstant(string name, uint value) => _symbolTable[name] = value;

        private IInstruction BindInstruction(string m, string[] args, uint pc) {
            var mClean = m.Replace(".", "").Replace("_", "");
            var method = _instMethods.FirstOrDefault(mi => mi.Name.Equals(mClean, StringComparison.OrdinalIgnoreCase));
            if (method == null) throw new Exception($"Unknown instruction {m}");
            
            // Reordering for Stores: sb rs2, off(rs1) -> [rs1, rs2, off]
            if (mClean.EndsWith("sw", StringComparison.OrdinalIgnoreCase) || 
                mClean.EndsWith("sb", StringComparison.OrdinalIgnoreCase) || 
                mClean.EndsWith("sd", StringComparison.OrdinalIgnoreCase) ||
                mClean.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length >= 3) {
                    var rs2 = args[0];
                    var imm = args[1];
                    var rs1 = args[2];
                    args = new[] { rs1, rs2, imm };
                }
            }
            // Reordering for Loads: lb rd, off(rs1) -> [rd, rs1, off]
            else if (mClean.StartsWith("l", StringComparison.OrdinalIgnoreCase) && 
                    !mClean.Equals("li", StringComparison.OrdinalIgnoreCase) && 
                    !mClean.Equals("la", StringComparison.OrdinalIgnoreCase) && 
                    !mClean.Equals("lui", StringComparison.OrdinalIgnoreCase))
            {
                // Matches Lb, Lh, Lw, Ld, Lbu, Lhu, Lwu
                if (args.Length >= 3) {
                    var rd = args[0];
                    var imm = args[1];
                    var rs1 = args[2];
                    args = new[] { rd, rs1, imm };
                }
            }
            // Reordering for CSRs: csrrw rd, csr, rs1 -> [rd, rs1, csr]
            else if (mClean.StartsWith("Csr", StringComparison.OrdinalIgnoreCase) && 
                    !mClean.Contains("i")) // Exclude Immediate variants which are naturally [rd, uimm, csr] in ASM -> [rd, rs1, imm]
            {
                // csrrw rd, csr, rs1. Constructor expects (rd, rs1, imm)
                if (args.Length >= 3) {
                    var rd = args[0];
                    var csr = args[1];
                    var rs1 = args[2];
                    args = new[] { rd, rs1, csr };
                }
            }

            var parameters = method.GetParameters();
            var convertedArgs = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                if (i >= args.Length) { convertedArgs[i] = 0; continue; }
                
                var pType = parameters[i].ParameterType;
                var argStr = args[i];

                try { 
                    int reg = RegisterAlias.Parse(argStr);
                    convertedArgs[i] = Convert.ChangeType(reg, pType); 
                    continue; 
                } catch { }

                if (_symbolTable.TryGetValue(argStr, out uint addr)) {
                    long val = (method.Name.StartsWith("B") || method.Name.StartsWith("Jal")) ? (long)addr - (long)pc : (long)addr;
                    convertedArgs[i] = Convert.ChangeType(val, pType);
                    continue;
                }

                long imm = ParseImmediate(argStr);
                
                // FIX: LUI and AUIPC need shifting if the immediate is small
                if ((method.Name.Equals("Lui", StringComparison.OrdinalIgnoreCase) || 
                     method.Name.Equals("Auipc", StringComparison.OrdinalIgnoreCase)) && i == 1)
                {
                    if ((imm & 0xFFFFF000) == 0 && imm != 0)
                    {
                        imm <<= 12;
                    }
                }

                convertedArgs[i] = Convert.ChangeType(imm, pType);
            }
            return (IInstruction)method.Invoke(null, convertedArgs)!;
        }

        private IEnumerable<IInstruction>? TryExpandPseudo(string m, string[] args, uint pc) {
            string mu = m.ToUpperInvariant();
            int ParseReg(int idx) => (idx < 0 || idx >= args.Length) ? 0 : RegisterAlias.Parse(args[idx]);
            long ResolveImm(int idx, bool pcRelative) {
                if (idx < 0 || idx >= args.Length) return 0;
                string tok = args[idx].Trim().TrimEnd(',');
                if (_symbolTable.TryGetValue(tok, out uint target)) return pcRelative ? (long)target - pc : (long)target;
                return ParseImmediate(tok);
            }
            
            int rd = 0, rs1 = 0, rs2 = 0; long imm = 0;
            bool isPseudo = true;

            try {
                switch (mu) {
                    case "LI": case "LA": rd = ParseReg(0); imm = ResolveImm(1, false); break;
                    case "MV": case "NOT": case "NEG": case "NEGW": case "SEXT.W": case "SEQZ": case "SNEZ": case "SLTZ": case "SGTZ": 
                        rd = ParseReg(0); rs1 = ParseReg(1); break;
                    case "BEQZ": case "BNEZ": case "BLEZ": case "BGEZ": case "BLTZ": case "BGTZ":
                        rs1 = ParseReg(0); imm = ResolveImm(1, true); break;
                    case "J": case "CALL": imm = ResolveImm(0, true); break;
                    case "NOP": case "RET": break;
                    default: isPseudo = false; break;
                }
            } catch { return null; }

            if (!isPseudo) return null;
            return PseudoExpander.Expand(mu, rd, rs1, rs2, imm);
        }

        private string[] Tokenize(string l) => Regex.Matches(l, @"0x[a-fA-F0-9]+|[\-0-9]+|[a-zA-Z0-9\._\-]+").Cast<Match>().Select(m => m.Value).ToArray();
        private string StripComments(string l) => l.Contains("#") ? l.Split('#')[0] : l;
        
        private long ParseImmediate(string s) {
            s = s.Trim().TrimEnd(',');
            if (RegisterAlias.TryParseCsr(s, out uint csr)) return csr;
            try {
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) 
                    return Convert.ToInt64(s.Substring(2), 16);
                return long.Parse(s);
            } catch { return 0; }
        }
    }
}
