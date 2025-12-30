#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AetherRISC.Generators
{
    [Generator]
    public class InstructionDecoderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) =>
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = context.SyntaxReceiver as SyntaxReceiver;
            if (receiver == null) return;

            var insts = new List<InstInfo>();

            foreach (var cls in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(cls.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(cls);

                var attr = symbol?.GetAttributes().FirstOrDefault(ad =>
                    ad.AttributeClass?.Name is "RiscvInstructionAttribute" or "RiscvInstruction");

                if (attr == null) continue;

                insts.Add(new InstInfo
                {
                    FullName = symbol!.ToDisplayString(),
                    Mnemonic = attr.ConstructorArguments[0].Value?.ToString() ?? "",
                    Arch = (int)attr.ConstructorArguments[1].Value!,
                    Type = (int)attr.ConstructorArguments[2].Value!,
                    Opcode = attr.ConstructorArguments[3].Value?.ToString() ?? "0",
                    Funct3 = GetNamedArg(attr, "Funct3"),
                    Funct7 = GetNamedArg(attr, "Funct7"),
                    Funct6 = GetNamedArg(attr, "Funct6"),
                    Rs2Sel = GetNamedArg(attr, "Rs2Sel")
                });
            }

            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using AetherRISC.Core.Architecture.Hardware.ISA;");
            sb.AppendLine("using AetherRISC.Core.Abstractions.Interfaces;");
            sb.AppendLine("using AetherRISC.Core.Architecture.Hardware.ISA.Utils;"); 
            sb.AppendLine();
            sb.AppendLine("namespace AetherRISC.Core.Architecture.Hardware.ISA.Decoding");
            sb.AppendLine("{");
            sb.AppendLine("    public partial class InstructionDecoder");
            sb.AppendLine("    {");
            sb.AppendLine("        private partial IInstruction? DecodeGenerated(uint raw, InstructionSet enabledSets)");
            sb.AppendLine("        {");
            sb.AppendLine("            int f3 = (int)((raw >> 12) & 0x7);");
            sb.AppendLine("            int f7 = (int)((raw >> 25) & 0x7F);");
            sb.AppendLine("            int rd = (int)((raw >> 7) & 0x1F);");
            sb.AppendLine("            int rs1 = (int)((raw >> 15) & 0x1F);");
            sb.AppendLine("            int rs2 = (int)((raw >> 20) & 0x1F);");
            sb.AppendLine("            int rs3 = (int)((raw >> 27) & 0x1F);"); 
            sb.AppendLine("            int immI = (int)raw >> 20;"); 
            sb.AppendLine();
            sb.AppendLine("            switch (raw & 0x7F)");
            sb.AppendLine("            {");

            foreach (var group in insts.GroupBy(x => x.Opcode))
            {
                sb.AppendLine($"                case {group.Key}:");
                foreach (var inst in group)
                {
                    var archFlag = $"(InstructionSet){inst.Arch}";
                    sb.AppendLine($"                    if (enabledSets.HasFlag({archFlag}))");
                    sb.AppendLine("                    {");

                    var checks = BuildChecks(inst);
                    var (immCode, args) = GetArgsForType(inst.Type);

                    if (!string.IsNullOrEmpty(immCode))
                    {
                        sb.AppendLine($"                        {{");
                        sb.AppendLine($"                            {immCode}");
                        sb.AppendLine($"                            if ({checks}) return new {inst.FullName}({args});");
                        sb.AppendLine($"                        }}");
                    }
                    else
                    {
                        sb.AppendLine($"                        if ({checks}) return new {inst.FullName}({args});");
                    }
                    sb.AppendLine("                    }");
                }
                sb.AppendLine("                    break;");
            }

            sb.AppendLine("            }");
            sb.AppendLine("            return null;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("InstructionDecoder.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private static string BuildChecks(InstInfo inst)
        {
            var checks = new List<string>();
            if (inst.Funct3 != null) checks.Add($"f3 == {inst.Funct3}");
            if (inst.Funct7 != null && inst.Type != 6) checks.Add($"f7 == {inst.Funct7}"); 
            if (inst.Type == 7 && inst.Funct6 != null) checks.Add($"((raw >> 26) & 0x3F) == {inst.Funct6}");
            if (inst.Type == 8 && inst.Rs2Sel != null) checks.Add($"rs2 == {inst.Rs2Sel}");
            
            if (inst.Mnemonic == "EBREAK") checks.Add("immI == 1");
            if (inst.Mnemonic == "ECALL") checks.Add("immI == 0");

            return checks.Count > 0 ? string.Join(" && ", checks) : "true";
        }

        private static (string immCode, string args) GetArgsForType(int type)
        {
            return type switch
            {
                0 => ("", "rd, rs1, rs2"), 
                1 => ("int imm = BitUtils.ExtractITypeImm(raw);", "rd, rs1, imm"), 
                2 => ("int imm = BitUtils.ExtractSTypeImm(raw);", "rs1, rs2, imm"), 
                3 => ("int imm = BitUtils.ExtractBTypeImm(raw);", "rs1, rs2, imm"), 
                4 => ("int imm = BitUtils.ExtractUTypeImm(raw);", "rd, imm"), 
                5 => ("int imm = BitUtils.ExtractJTypeImm(raw);", "rd, imm"), 
                6 => ("", "rd, rs1, rs2, rs3"),
                7 => ("int imm = BitUtils.ExtractShamt(raw, 64);", "rd, rs1, imm"), 
                8 => ("", "rd, rs1, 0"), 
                _ => ("", "rd, rs1, rs2")
            };
        }

        private static string? GetNamedArg(AttributeData attr, string name)
        {
            var kvp = attr.NamedArguments.FirstOrDefault(x => x.Key == name);
            return kvp.Key == name ? kvp.Value.Value?.ToString() : null;
        }

        private class InstInfo { public string FullName="", Mnemonic="", Opcode=""; public string? Funct3, Funct7, Funct6, Rs2Sel; public int Arch, Type; }
        private class SyntaxReceiver : ISyntaxReceiver { public List<ClassDeclarationSyntax> CandidateClasses { get; } = new(); public void OnVisitSyntaxNode(SyntaxNode s) { if (s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0) CandidateClasses.Add(c); } }
    }
}
