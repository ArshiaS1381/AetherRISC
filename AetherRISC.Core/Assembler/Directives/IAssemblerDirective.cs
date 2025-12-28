namespace AetherRISC.Core.Assembler.Directives
{
    /// <summary>
    /// Represents a command to the assembler that is not a machine instruction.
    /// Used for defining data, controlling memory layout, or setting symbols.
    /// </summary>
    public interface IAssemblerDirective
    {
        /// <summary>
        /// The primary name of the directive (e.g., ".text").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A plain English explanation of what this directive does.
        /// Useful for educational tooltips.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The expected argument format (e.g., ".word value, ...").
        /// </summary>
        string Usage { get; }

        /// <summary>
        /// Checks if a source token matches this directive.
        /// </summary>
        bool Match(string token);

        /// <summary>
        /// Executes the directive logic, modifying the assembler state.
        /// </summary>
        void Execute(SourceAssembler assembler, string[] args);
    }
}
