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

namespace AetherRISC.Core.Assembler;

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
        _instMethods = typeof(Inst).GetMethods(BindingFlags.Public | BindingFlags.Static);
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

        // Pass 1
        IsFirstPass = true;
        foreach (var rawLine in _lines)
        {
            var line = StripComments(rawLine);
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.Contains(":")) {
                var parts = line.Split(':');
                _symbolTable[parts[0].Trim()] = InTextSection ? CurrentTextPtr : CurrentDataPtr;
                line = parts.Last().Trim();
            }
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith(".")) HandleDirective(line);
            else if (InTextSection) {
                var tokens = Tokenize(line);
                if (tokens.Length == 0) continue;
                var m = tokens[0].ToUpperInvariant();
                if (m == "LA" || m == "CALL") CurrentTextPtr += 8u;
                else if (m == "LI") {
                    long immVal = 0;
                    bool ok = tokens.Length >= 3 && TryParseNumber(tokens[2], out immVal);
                    if (ok && immVal >= -2048 && immVal <= 2047) CurrentTextPtr += 4u;
                    else CurrentTextPtr += 8u;
                } else CurrentTextPtr += 4u;
            }
        }

        // Pass 2
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

            if (line.StartsWith(".")) {
                HandleDirective(line);
                continue;
            }

            var tokens = Tokenize(line);
            if (tokens.Length == 0) continue;

            var mRaw = tokens[0];
            var mUpper = mRaw.ToUpperInvariant();
            var mBind = NormalizeMnemonicForInstLookup(mRaw);
            var args = tokens.Skip(1).ToArray();

            if (args.Length == 2 && args[1].Contains("(")) {
                var parts = args[1].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                args = new[] { args[0], parts[1], parts[0] };
            }

            // CRITICAL FIX: Do NOT catch exceptions blindly. 
            // If it's a known pseudo, let parsing errors bubble up.
            var expanded = TryExpandPseudo(mUpper, args, CurrentTextPtr);
            
            var toEncode = expanded?.ToList() ?? new List<IInstruction> { BindInstruction(mBind, args, CurrentTextPtr) };

            foreach (var inst in toEncode) {
                state.Memory!.WriteWord(CurrentTextPtr, InstructionEncoder.Encode(inst));
                CurrentTextPtr += 4;
            }
        }
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
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return long.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out result);
        return long.TryParse(s, out result);
    }

    public uint ResolveSymbol(string s) => _symbolTable.TryGetValue(s, out uint val) ? val : 0;
    public void DefineConstant(string name, uint value) => _symbolTable[name] = value;

    private static string NormalizeMnemonicForInstLookup(string mnemonic) {
        if (string.IsNullOrWhiteSpace(mnemonic)) return mnemonic;
        return mnemonic.Replace(".", "").Replace("_", "");
    }

    private IInstruction BindInstruction(string m, string[] args, uint pc) {
        var method = _instMethods.FirstOrDefault(mi => mi.Name.Equals(m, StringComparison.OrdinalIgnoreCase));
        if (method == null) throw new Exception($"Unknown instruction {m}");
        if (method.Name.StartsWith("Csrr") && args.Length >= 3) { var tmp = args[1]; args[1] = args[2]; args[2] = tmp; }
        if (m.Equals("SB", StringComparison.OrdinalIgnoreCase) || m.Equals("SW", StringComparison.OrdinalIgnoreCase) || m.Equals("SD", StringComparison.OrdinalIgnoreCase))
        { if (args.Length >= 2) { var temp = args[0]; args[0] = args[1]; args[1] = temp; } }

        var parameters = method.GetParameters();
        var convertedArgs = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++) {
            if (i >= args.Length) { convertedArgs[i] = GetDefault(parameters[i].ParameterType); continue; }
            try { convertedArgs[i] = Convert.ChangeType(RegisterAlias.Parse(args[i]), parameters[i].ParameterType); continue; } catch { }
            if (_symbolTable.TryGetValue(args[i], out uint addr)) {
                long val = (method.Name.StartsWith("B") || method.Name == "Jal") ? (int)addr - (int)pc : (int)addr;
                convertedArgs[i] = Convert.ChangeType(val, parameters[i].ParameterType); continue;
            }
            long imm = ParseImmediate(args[i]);
            if ((method.Name.Equals("Lui", StringComparison.OrdinalIgnoreCase) || method.Name.Equals("Auipc", StringComparison.OrdinalIgnoreCase)) && i == 1) imm <<= 12;
            convertedArgs[i] = Convert.ChangeType(imm, parameters[i].ParameterType);
        }
        return (IInstruction)method.Invoke(null, convertedArgs)!;
    }

    private static object GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t)! : 0;

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
                case "MV": case "NOT": case "NEG": case "NEGW": case "SEXT.W": case "SEXT.B": 
                case "SEXT.H": case "ZEXT.H": case "SEQZ": case "SNEZ": case "SLTZ": case "SGTZ": 
                case "FMV.S": case "FABS.S": case "FNEG.S": case "FMV.D": case "FABS.D": case "FNEG.D": case "ORC.B":
                    rd = ParseReg(0); rs1 = ParseReg(1); break;
                case "BEQZ": case "BNEZ": case "BLEZ": case "BGEZ": case "BLTZ": case "BGTZ":
                    rs1 = ParseReg(0); imm = ResolveImm(1, true); break;
                case "BGT": case "BLE": case "BGTU": case "BLEU":
                    rs1 = ParseReg(0); rs2 = ParseReg(1); imm = ResolveImm(2, true); break;
                case "J": case "CALL": imm = ResolveImm(0, true); break;
                case "RET": break;
                default: isPseudo = false; break;
            }
        }
        catch (Exception ex)
        {
            // If it WAS a pseudo mnemonic (isPseudo=true), but parsing failed (e.g. label not found),
            // we MUST throw here to report the real error.
            if (isPseudo) throw new Exception($"Failed to parse arguments for pseudo-instruction '{mu}': {ex.Message}", ex);
            return null;
        }

        if (!isPseudo) return null;

        var res = PseudoExpander.Expand(mu, rd, rs1, rs2, imm);
        if (!res.Any()) throw new Exception($"Pseudo-instruction '{mu}' recognized but implementation not found. Check PseudoExpander.");
        return res;
    }

    private string[] Tokenize(string l) => Regex.Matches(l, @"0x[a-fA-F0-9]+|[a-zA-Z0-9\._\-]+\([a-zA-Z0-9\._\-]+\)|[a-zA-Z0-9\._\-]+").Cast<Match>().Select(m => m.Value.Trim('(', ')')).ToArray();
    private string StripComments(string l) => l.Contains("#") ? l.Split('#')[0] : l;
    private long ParseImmediate(string s) {
        s = s.Trim().TrimEnd(',');
        if (RegisterAlias.TryParseCsr(s, out uint csr)) return csr;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return unchecked((int)uint.Parse(s.Substring(2), NumberStyles.HexNumber));
        if (int.TryParse(s, out int val)) return val;
        throw new FormatException($"Invalid immediate or missing label: '{s}'");
    }
}
