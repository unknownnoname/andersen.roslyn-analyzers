using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RoslynCodeAnalyzers.Providers
{
    public abstract class CodeFixProviderBase : CodeFixProvider
    {
        protected abstract string DiagnosticId { get; }

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    }
}