using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynCodeAnalyzers.Analyzers
{
    public abstract class RoslynAnalyzerBase : DiagnosticAnalyzer
    {
        protected abstract string Title { get; }
        
        protected abstract string MessageFormat { get; }
        
        protected abstract string Description { get; }
        
        protected abstract string Category { get; }
        
        protected DiagnosticDescriptor Rule => new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected abstract void RegisterRuleAction(AnalysisContext context);

        public abstract string DiagnosticId { get; }
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            RegisterRuleAction(context);
        }
    }
}