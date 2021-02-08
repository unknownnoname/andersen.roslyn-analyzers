using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCodeAnalyzers.Analyzers;

namespace RoslynCodeAnalyzers.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConfigureAwaitCodeFixProvider)), Shared]
    public class ConfigureAwaitCodeFixProvider : CodeFixProviderBase
    {
        private const string ConfigureAwaitMethodName = nameof(Task.CompletedTask.ConfigureAwait);
        private const string Title = "Call 'ConfigureAwait(false)'";
        
        protected override string DiagnosticId => new ConfigureAwaitAnalyzer().DiagnosticId;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            // Find the await expression identified by the diagnostic.
            var awaitExpression = root.FindNode(diagnostic.Location.SourceSpan).AncestorsAndSelf().OfType<AwaitExpressionSyntax>().FirstOrDefault();
            if (awaitExpression == null)
            {
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => Fix(context.Document, awaitExpression, c),
                    Title),
                diagnostic);
        }

        private async Task<Document> Fix(Document document, AwaitExpressionSyntax awaitExpressionSyntax, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var predefinedConfigureAwaitArgument = SyntaxFactory.SingletonSeparatedList(
                       SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

            var configureAwaitInvocationExpression = awaitExpressionSyntax
               .DescendantNodes()
               .OfType<MemberAccessExpressionSyntax>()
               .LastOrDefault(node => node.ChildNodes().OfType<IdentifierNameSyntax>()
                    .Any(child => child.Identifier.ValueText == ConfigureAwaitMethodName));

            // Change 'ConfigureAwait(true)' to 'ConfigureAwait(false)' if its present.
            if (configureAwaitInvocationExpression != null)
            {
                var configureAwaitParentInvocationExpression = (InvocationExpressionSyntax)configureAwaitInvocationExpression.Parent;

                var newInvocationExpression = configureAwaitParentInvocationExpression.WithArgumentList(
                    SyntaxFactory.ArgumentList(predefinedConfigureAwaitArgument));

                root = root.ReplaceNode(configureAwaitParentInvocationExpression, newInvocationExpression);

                return document.WithSyntaxRoot(root);
            }

            // Create new syntax with 'ConfigureAwait(false)' method call.
            InvocationExpressionSyntax expressionSyntaxWithConfigureAwait = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    awaitExpressionSyntax,
                    SyntaxFactory.IdentifierName(ConfigureAwaitMethodName)),
                SyntaxFactory.ArgumentList(predefinedConfigureAwaitArgument));

            // Insert 'ConfigureAwait(false)' method call.
            root = root.ReplaceNode(awaitExpressionSyntax, expressionSyntaxWithConfigureAwait);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(root);
        }
    }
}
