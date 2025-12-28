using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AetherRISC.Generators
{
    [Generator]
    public class InstructionEncoderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using AetherRISC.Core.Abstractions.Interfaces;");
            // No longer need ISA.Attributes, attributes are in ISA root
            sb.AppendLine("using AetherRISC.Core.Architecture.Hardware.ISA;"); 
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine("namespace AetherRISC.Core.Architecture.Hardware.ISA.Encoding");
            sb.AppendLine("{");
            sb.AppendLine("    public static partial class InstructionEncoder");
            sb.AppendLine("    {");
            sb.AppendLine("        static partial void RegisterGenerated()");
            sb.AppendLine("        {");

            var receiver = context.SyntaxReceiver as SyntaxReceiver;
            if (receiver != null)
            {
                foreach (var classDeclaration in receiver.CandidateClasses)
                {
                    var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                    var symbol = model.GetDeclaredSymbol(classDeclaration);
                    
                    var attr = symbol.GetAttributes().FirstOrDefault(ad => 
                        ad.AttributeClass.Name == "RiscvInstructionAttribute" || 
                        ad.AttributeClass.Name == "RiscvInstruction");

                    if (attr != null) GenerateRegistration(sb, attr);
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("InstructionEncoder.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private void GenerateRegistration(StringBuilder sb, AttributeData attr)
        {
            var args = attr.ConstructorArguments;
            var mnemonic = args[0].Value.ToString();
            // Skip Arch (args[1])
            int typeVal = (int)args[2].Value;
            var opcode = args[3].Value.ToString();

            string f3 = GetNamedArg(attr, "Funct3", "0");
            string f7 = GetNamedArg(attr, "Funct7", "0");
            string f6 = GetNamedArg(attr, "Funct6", "0");
            string rs2Sel = GetNamedArg(attr, "Rs2Sel", "0");

            // Mapping based on RiscvEncodingType enum order: R, I, S, B, U, J, R4, ShiftImm, ZbbUnary
            string call = typeVal switch
            {
                0 => $"GenR({opcode}, {f3}, {f7}, inst)",
                1 => $"GenI({opcode}, {f3}, inst)",
                2 => $"GenS({opcode}, {f3}, inst)",
                3 => $"GenB({opcode}, {f3}, inst)",
                4 => $"GenU({opcode}, inst)",
                5 => $"GenJ({opcode}, inst)",
                7 => $"GenShiftImm({opcode}, {f3}, {f6}, inst)",
                8 => $"GenZbbUnary({opcode}, {f3}, {f7}, {rs2Sel}, inst)",
                _ => "0"
            };

            sb.AppendLine($"            Register(\"{mnemonic}\", inst => {call});");
        }

        private string GetNamedArg(AttributeData attr, string name, string def)
        {
            var kvp = attr.NamedArguments.FirstOrDefault(x => x.Key == name);
            return kvp.Key == name ? kvp.Value.Value.ToString() : def;
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();
            public void OnVisitSyntaxNode(SyntaxNode s)
            {
                if (s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0) CandidateClasses.Add(c);
            }
        }
    }
}
