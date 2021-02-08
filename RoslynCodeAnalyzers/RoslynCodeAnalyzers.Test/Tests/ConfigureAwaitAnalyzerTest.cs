using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = IHS.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    RoslynCodeAnalyzers.Analyzers.ConfigureAwaitAnalyzer,
    RoslynCodeAnalyzers.Providers.ConfigureAwaitCodeFixProvider>;

namespace RoslynCodeAnalyzers.Test.Tests
{
    [TestClass]
    public class ConfigureAwaitAnalyzerTest
    {
        [TestMethod]
        public async Task Test_NoDiagnosticsExpected()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task Test_DiagnosticsAndFixExpected()
        {
            var test = @"
            using System.Threading.Tasks;

            namespace ConsoleApp1
            {
                class Program
                {
                    static async Task Main()
                    {
                        await GetListAsync();
                    }

                    static Task GetListAsync()
                    {
                        return Task.CompletedTask;
                    }
                }
            }";

            var fixtest = @"
            using System.Threading.Tasks;

            namespace ConsoleApp1
            {
                class Program
                {
                    static async Task Main()
                    {
                        await GetListAsync().ConfigureAwait(false);
                    }

                    static Task GetListAsync()
                    {
                        return Task.CompletedTask;
                    }
                }
            }";

            var expected = VerifyCS.Diagnostic(RoslynDiagnosticIdentifiers.R01);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
