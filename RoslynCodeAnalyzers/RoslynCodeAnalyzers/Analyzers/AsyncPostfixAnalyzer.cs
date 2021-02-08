using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynCodeAnalyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncPostfixAnalyzer : RoslynAnalyzerBase
    {
        private const string AsyncPostfix = "Async";
        private const string TaskPrefix = "Task";

        protected override string Title => "'Async' postfix is missing";

        protected override string MessageFormat => "Rename method to have 'Async' postfix";

        protected override string Description => "Methods that returns Task or Task<T> should have 'Async' postfix in name declaration";

        protected override string Category => "EWB";
        
        public override string DiagnosticId => RoslynDiagnosticIdentifiers.R02;

        protected override void RegisterRuleAction(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclarationNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethodDeclarationNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

            var returnTypeText = methodDeclarationSyntax.ReturnType.GetText().ToString();
            var methodNameIdentifier = methodDeclarationSyntax.Identifier;

            if(returnTypeText.StartsWith(TaskPrefix) && !methodNameIdentifier.Text.EndsWith(AsyncPostfix))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, methodNameIdentifier.GetLocation()));
            }
        }
    }
}