// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable.Analyzer;
using Josupeit.Practices.Mixins.Disposable.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Josupeit.Practices.Mixins.Disposable.Tests;

[TestFixture]
[TestOf(typeof(ProtectedVirtualVisibilityCodeFixProvider))]
internal sealed class VisibilityCodeFixTests
{
    [Test]
    public async Task JPMD040_makes_method_protected_virtual()
    {
        var test = new CSharpCodeFixVerifier<ProtectedVirtualVisibilityCodeFixProvider>.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#1:TestClass|}
    {
        public void {|#0:DisposeManagedResources|}() { }
    }
}
",

            FixedCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class {|#1:TestClass|}
    {
        protected virtual void DisposeManagedResources() { }
    }
}
",

            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD040", DiagnosticSeverity.Warning)
                    .WithLocation(0)
                    .WithArguments("TestClass", "DisposeManagedResources"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                    .WithLocation(1)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD034", DiagnosticSeverity.Hidden)
                    .WithLocation(1)
                    .WithArguments("TestClass"),
            },
        };

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                .WithLocation(1)
                .WithArguments("TestClass"));

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD034", DiagnosticSeverity.Hidden)
                .WithLocation(1)
                .WithArguments("TestClass"));

        await test.RunAsync();
    }

    [Test]
    public async Task JPMD041_makes_method_private()
    {
        var test = new CSharpCodeFixVerifier<PrivateVisibilityCodeFixProvider>.Test
        {
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public sealed partial class {|#1:TestClass|}
    {
        protected void {|#0:DisposeManagedResources|}() { }
    }
}
",

            FixedCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public sealed partial class {|#1:TestClass|}
    {
        private void DisposeManagedResources() { }
    }
}
",
            ExpectedDiagnostics =
            {
                new DiagnosticResult("JPMD041", DiagnosticSeverity.Warning)
                    .WithLocation(0)
                    .WithArguments("TestClass", "DisposeManagedResources"),

                new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                    .WithLocation(1)
                    .WithArguments("TestClass"),

                new DiagnosticResult("JPMD034", DiagnosticSeverity.Hidden)
                    .WithLocation(1)
                    .WithArguments("TestClass"),
            },
        };

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD033", DiagnosticSeverity.Hidden)
                .WithLocation(1)
                .WithArguments("TestClass"));

        test.FixedState.ExpectedDiagnostics.Add(
            new DiagnosticResult("JPMD034", DiagnosticSeverity.Hidden)
                .WithLocation(1)
                .WithArguments("TestClass"));

        await test.RunAsync();
    }

    [Test]
    public async Task No_diagnostic_when_visibility_is_correct()
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
}
