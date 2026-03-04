using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable.Analyzer;
using Josupeit.Practices.Mixins.Disposable.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Josupeit.Practices.Mixins.Disposable.Tests;

[TestFixture]
[TestOf(typeof(DisposeManagedResourcesCodeFixProvider))]
internal sealed class CallbackCodeFixTests
{
    [Test]
    public async Task JPMD032_adds_DisposeManagedResources_to_non_sealed()
    {
        var test = new CSharpCodeFixVerifier<DisposeManagedResourcesCodeFixProvider>.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#0:TestClass|}
    {
        protected virtual void DisposeUnmanagedResources() { }
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
        protected virtual void DisposeUnmanagedResources() { }

        /// <summary>
        /// Performs application-defined task associated with freeing, releasing, or resetting managed resources.
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
        }
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("TestClass"));

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD032_adds_DisposeManagedResources_to_sealed()
    {
        var test = new CSharpCodeFixVerifier<DisposeManagedResourcesCodeFixProvider>.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public sealed partial class {|#0:TestClass|}
    {
        private void DisposeUnmanagedResources() { }
    }
}
",

            FixedCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public sealed partial class {|#0:TestClass|}
    {
        private void DisposeUnmanagedResources() { }

        /// <summary>
        /// Performs application-defined task associated with freeing, releasing, or resetting managed resources.
        /// </summary>
        private void DisposeManagedResources()
        {
        }
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("TestClass"));

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD033_adds_DisposeManagedResourcesAsync_to_non_sealed()
    {
        var test = new CSharpCodeFixVerifier<DisposeManagedResourcesAsyncCodeFixProvider>.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#0:TestClass|}
    {
        protected virtual void DisposeUnmanagedResources() { }
    }
}
",

            FixedCode = @"
using Josupeit.Practices.Mixins.Disposable;
using System.Threading.Tasks;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#0:TestClass|}
    {
        protected virtual void DisposeUnmanagedResources() { }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources asynchronously.
        /// </summary>
        /// <returns>
        /// A <see cref=""ValueTask""/> that completes when all asynchronous cleanup operations have finished.
        /// </returns>
        protected virtual ValueTask DisposeManagedResourcesAsync()
        {
            return new ValueTask();
        }
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("TestClass"));

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD033_not_reported_with_WithoutAsyncDisposable()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithoutAsyncDisposable)]
    public partial class {|#0:TestClass|}
    {
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD031", DiagnosticSeverity.Info)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD034", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD034_adds_DisposeUnmanagedResources()
    {
        var test = new CSharpCodeFixVerifier<DisposeUnmanagedResourcesCodeFixProvider>.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#0:TestClass|}
    {
        protected virtual void DisposeManagedResources() { }
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
        protected virtual void DisposeManagedResources() { }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void DisposeUnmanagedResources()
        {
        }
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD034", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("TestClass"));

        await test.RunAsync();
    }
}
