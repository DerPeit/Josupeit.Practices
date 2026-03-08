// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Josupeit.Practices.Mixins.Disposable.Tests;

[TestFixture]
[TestOf(typeof(DisposableSourceGenerator))]
internal sealed class DisposableMixinBehaviorTests
{
    private const string TEST_SOURCE = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithEvent | DisposableMixinOptions.WithProperty)]
    public partial class TestClass
    {
        private readonly List<string> _callLog = new List<string>();

        public TestClass()
        {
            this.Disposing += this.OnDisposingHandler;
        }

        public List<string> CallLog
        {
            get { return this._callLog; }
        }

        public void CallThrowIfDisposed()
        {
            this.ThrowIfDisposed();
        }

        protected virtual void DisposeManagedResources()
        {
            this._callLog.Add(""DisposeManagedResources"");
        }

        protected virtual ValueTask DisposeManagedResourcesAsync()
        {
            this._callLog.Add(""DisposeManagedResourcesAsync"");
            return new ValueTask();
        }

        protected virtual void DisposeUnmanagedResources()
        {
            this._callLog.Add(""DisposeUnmanagedResources"");
        }

        private void OnDisposingHandler(object sender, EventArgs e)
        {
            this._callLog.Add(""Disposing"");
        }
    }
}
";

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task Dispose_sets_IsDisposed_to_true(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        Assert.That((bool)instance.IsDisposed, Is.False);

        ((IDisposable)instance).Dispose();

        Assert.That((bool)instance.IsDisposed, Is.True);
    }

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task Dispose_calls_callbacks_in_correct_order(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        ((IDisposable)instance).Dispose();

        Assert.That(
            (IList<string>)instance.CallLog,
            Is.EqualTo(["Disposing", "DisposeManagedResources", "DisposeUnmanagedResources"]));
    }

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task Dispose_is_idempotent(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        ((IDisposable)instance).Dispose();
        ((IDisposable)instance).Dispose();

        Assert.That(
            (IList<string>)instance.CallLog,
            Is.EqualTo(["Disposing", "DisposeManagedResources", "DisposeUnmanagedResources"]));
    }

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task DisposeAsync_sets_IsDisposed_to_true(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        Assert.That((bool)instance.IsDisposed, Is.False);

        await ((IAsyncDisposable)instance).DisposeAsync();

        Assert.That((bool)instance.IsDisposed, Is.True);
    }

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task DisposeAsync_calls_callbacks_in_correct_order(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        await ((IAsyncDisposable)instance).DisposeAsync();

        Assert.That(
            (IList<string>)instance.CallLog,
            Is.EqualTo(["Disposing", "DisposeManagedResourcesAsync", "DisposeUnmanagedResources"]));
    }

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task DisposeAsync_is_idempotent(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        await ((IAsyncDisposable)instance).DisposeAsync();
        await ((IAsyncDisposable)instance).DisposeAsync();

        Assert.That(
            (IList<string>)instance.CallLog,
            Is.EqualTo(["Disposing", "DisposeManagedResourcesAsync", "DisposeUnmanagedResources"]));
    }

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task ThrowIfDisposed_throws_ObjectDisposedException_after_Dispose(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        ((IDisposable)instance).Dispose();

        Assert.That(() => { instance.CallThrowIfDisposed(); }, Throws.InstanceOf<ObjectDisposedException>());
    }

    [TestCaseSource(nameof(GetLanguageVersions))]
    public async Task ThrowIfDisposed_does_not_throw_before_Dispose(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var type = await CompileAndLoadType(languageVersion, referenceAssemblies);
        dynamic instance = Activator.CreateInstance(type)!;

        Assert.That(() => { instance.CallThrowIfDisposed(); }, Throws.Nothing);
    }

    private static IEnumerable<TestCaseData> GetLanguageVersions()
    {
        yield return new TestCaseData(LanguageVersion.CSharp12, ReferenceAssemblies.Net.Net80).SetName("{m}_CSharp12");
        yield return new TestCaseData(LanguageVersion.CSharp5, ReferenceAssemblies.NetStandard.NetStandard21).SetName("{m}_CSharp5");
    }

    private static async Task<Type> CompileAndLoadType(LanguageVersion languageVersion, ReferenceAssemblies referenceAssemblies)
    {
        var references = await referenceAssemblies.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);

        var parseOptions = new CSharpParseOptions(languageVersion);
        var syntaxTree = CSharpSyntaxTree.ParseText(TEST_SOURCE, parseOptions);

        var compilation = CSharpCompilation.Create(
            $"BehaviorTest_{languageVersion}",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        CSharpGeneratorDriver
            .Create(new DisposableSourceGenerator())
            .WithUpdatedParseOptions(parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var compilationErrors = outputCompilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        Assert.That(compilationErrors, Is.Empty, "Compilation should succeed without errors");

        using var stream = new MemoryStream();
        var emitResult = outputCompilation.Emit(stream);

        Assert.That(emitResult.Success, Is.True, "Emit should succeed");

        var assembly = Assembly.Load(stream.ToArray());
        var type = assembly.GetType("TestNamespace.TestClass");

        Assert.That(type, Is.Not.Null, "TestClass should exist in the compiled assembly");

        return type!;
    }
}
