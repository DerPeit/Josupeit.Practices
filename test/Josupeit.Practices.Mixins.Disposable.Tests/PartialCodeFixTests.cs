using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable.Analyzer;
using Josupeit.Practices.Mixins.Disposable.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Josupeit.Practices.Mixins.Disposable.Tests;

[TestFixture]
[TestOf(typeof(PartialCodeFixProvider))]
internal sealed class PartialCodeFixTests
{
    [Test]
    public async Task JPMD020_adds_partial_to_non_partial_class()
    {
        var test = new CSharpCodeFixVerifier<PartialCodeFixProvider>.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public class {|#0:TestClass|}
    {
    }
}
",

            FixedCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#0:TestClass|}
    {
    }
}
",
            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD020", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD031", DiagnosticSeverity.Info)
                .WithLocation(0)
                .WithArguments("TestClass"));

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("TestClass"));

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("TestClass"));

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD034", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("TestClass"));

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD021_reports_error_for_non_partial_parent()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public class {|#0:Parent|}
    {
        [DisposableMixin]
        public partial class TestClass
        {
        }
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD021", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("Parent"),
            },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task No_diagnostic_when_class_and_parents_are_partial()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public partial class Parent
    {
        [DisposableMixin]
        public partial class TestClass
        {
            protected virtual void DisposeManagedResources() { }
            protected virtual ValueTask DisposeManagedResourcesAsync() { return new ValueTask(); }
            protected virtual void DisposeUnmanagedResources() { }
        }
    }
}
",
        };

        await test.RunAsync();
    }
}
