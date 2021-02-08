using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynCodeAnalyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigureAwaitAnalyzer : RoslynAnalyzerBase
    {
        private const string ConfigureAwaitMethodName = nameof(Task.CompletedTask.ConfigureAwait);

        protected override string Title => "Missing 'ConfigureAwait(false)' call";

        protected override string MessageFormat => "Missing 'ConfigureAwait(false)' call";

        protected override string Description => "Missing 'ConfigureAwait(false)' call";

        protected override string Category => "EWB";

        public override string DiagnosticId => RoslynDiagnosticIdentifiers.R01;

        protected override void RegisterRuleAction(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAwaitExpressionNode, SyntaxKind.AwaitExpression);
        }

        private void AnalyzeAwaitExpressionNode(SyntaxNodeAnalysisContext context)
        {
            var awaitExpressionSyntax = (AwaitExpressionSyntax) context.Node;

            var invocationExpression = awaitExpressionSyntax.ChildNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocationExpression == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, awaitExpressionSyntax.GetLocation()));

                return;
            }

            var simpleMemberExpression = invocationExpression.ChildNodes().OfType<MemberAccessExpressionSyntax>().LastOrDefault();
            if (simpleMemberExpression == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocationExpression.GetLocation()));

                return;
            }

            var configureAwaitIdentifier = simpleMemberExpression
                .ChildNodes()
                .OfType<IdentifierNameSyntax>()
                .LastOrDefault(child => child.Identifier.ValueText == ConfigureAwaitMethodName);

            if (configureAwaitIdentifier == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocationExpression.GetLocation()));

                return;
            }
        }
    }
}