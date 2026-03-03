using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable.Analyzer;
using Josupeit.Practices.Mixins.Disposable.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Josupeit.Practices.Mixins.Disposable.Tests;

[TestFixture]
[TestOf(typeof(DisposableAnalyzer))]
internal sealed class DisposableAnalyzerTests
{
    [Test]
    public async Task JPMD010_reports_error_when_base_class_implements_IDisposable()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using System;
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public class DisposableBase : IDisposable
    {
        public void Dispose() { }
    }

    [{|#0:DisposableMixin|}]
    public partial class TestClass : DisposableBase
    {
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD010", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD010_does_not_report_when_no_disposable_base()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
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
                new DiagnosticResult("JPMD031", DiagnosticSeverity.Info)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
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
    public async Task JPMD030_warns_sealed_class_with_no_callbacks()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public sealed partial class {|#0:TestClass|}
    {
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD030", DiagnosticSeverity.Warning)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
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
    public async Task JPMD031_informs_non_sealed_class_with_no_callbacks()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
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
                new DiagnosticResult("JPMD031", DiagnosticSeverity.Info)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD032", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
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
    public async Task No_diagnostics_when_all_callbacks_implemented()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
        protected virtual void DisposeManagedResources() { }
        protected virtual ValueTask DisposeManagedResourcesAsync() { return new ValueTask(); }
        protected virtual void DisposeUnmanagedResources() { }
    }
}
",
        };

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD010_reports_error_with_indirect_disposable_base()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using System;
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public class DisposableBase : IDisposable
    {
        public void Dispose() { }
    }

    public class MiddleClass : DisposableBase
    {
    }

    [{|#0:DisposableMixin|}]
    public partial class TestClass : MiddleClass
    {
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD010", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD010_reports_error_when_base_class_implements_IAsyncDisposable()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using System;
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public class AsyncDisposableBase : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => default;
    }

    [{|#0:DisposableMixin|}]
    public partial class TestClass : AsyncDisposableBase
    {
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD010", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("TestClass"),
            },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task FindCallbacks_ignores_static_methods()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            CompilerDiagnostics = CompilerDiagnostics.None,
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#0:TestClass|}
    {
        public static void DisposeManagedResources() { }
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

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
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
    public async Task No_diagnostics_when_callbacks_in_separate_partial_file()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestState =
            {
                Sources =
                {
                    @"
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
",
                    @"
using System.Threading.Tasks;

namespace TestNamespace
{
    public partial class TestClass
    {
        protected virtual void DisposeManagedResources() { }
        protected virtual ValueTask DisposeManagedResourcesAsync() { return new ValueTask(); }
        protected virtual void DisposeUnmanagedResources() { }
    }
}
",
                },
            },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task Analyzer_reads_named_attribute_arguments()
    {
        var test = new CSharpAnalyzerVerifier.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(Options = DisposableMixinOptions.WithoutAsyncDisposable)]
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
}
