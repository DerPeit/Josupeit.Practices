// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable.Generator;
using Josupeit.Practices.Mixins.Disposable.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;

namespace Josupeit.Practices.Mixins.Disposable.Tests;

[TestFixture]
[TestOf(typeof(DisposableSourceGenerator))]
internal sealed class DisposableSourceGeneratorTests
{
    private const string ATTRIBUTES_HINT_NAME = "Attributes.g.cs";
    private const string NO_ATTRIBUTES_SYMBOL = "JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE_NO_ATTRIBUTES";
    private const string NO_EMBEDDED_ATTRIBUTE_SYMBOL = "JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE_NO_EMBEDDED_ATTRIBUTE";

    [Test]
    public async Task Generator_produces_full_implementation_with_all_options()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithEvent | DisposableMixinOptions.WithProperty)]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("Disposing"));
        Assert.That(generated, Does.Contain("IsDisposed"));
        Assert.That(generated, Does.Contain("IAsyncDisposable"));
        Assert.That(generated, Does.Contain("ThrowIfDisposed"));
        Assert.That(generated, Does.Contain("~TestClass"));
        Assert.That(generated, Does.Contain("GC.SuppressFinalize"));
        Assert.That(generated, Does.Contain("OnDisposing"));
    }

    [Test]
    public async Task Generator_omits_event_without_WithEvent()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithProperty)]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("event"));
        Assert.That(generated, Does.Not.Contain("OnDisposing"));
        Assert.That(generated, Does.Contain("IsDisposed"));
    }

    [Test]
    public async Task Generator_omits_property_without_WithProperty()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithEvent)]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("IsDisposed =>"));
        Assert.That(generated, Does.Contain("Disposing"));
    }

    [Test]
    public async Task Generator_produces_minimal_implementation_with_no_options()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("event"));
        Assert.That(generated, Does.Not.Contain("IsDisposed =>"));
        Assert.That(generated, Does.Contain("_isDisposed"));
        Assert.That(generated, Does.Contain("IAsyncDisposable"));
    }

    [Test]
    public async Task Generator_omits_async_with_WithoutAsyncDisposable()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithoutAsyncDisposable)]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("IAsyncDisposable"));
        Assert.That(generated, Does.Not.Contain("DisposeAsync"));
        Assert.That(generated, Does.Not.Contain("DisposeAsyncCore"));
        Assert.That(generated, Does.Not.Contain("DisposeManagedResourcesAsync"));
    }

    [Test]
    public async Task Generator_omits_idempotency_with_WithoutIdempotencyGuarantees()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithoutIdempotencyGuarantees)]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("_isDisposed"));
        Assert.That(generated, Does.Not.Contain("ThrowIfDisposed"));
    }

    [Test]
    public async Task Generator_removes_empty_finally_for_sealed_class_with_event()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithEvent)]
    public sealed partial class TestClass
    {
    }
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("OnDisposing"));
        Assert.That(generated, Does.Not.Contain("finally"));
    }

    [Test]
    public async Task Generator_makes_members_private_for_sealed_class()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(DisposableMixinOptions.WithProperty)]
    public sealed partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("~TestClass"));
        Assert.That(generated, Does.Not.Contain("protected virtual"));
        Assert.That(generated, Does.Contain("IsDisposed"));
    }

    [Test]
    public async Task Generator_skips_user_implemented_callbacks()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
        protected virtual void DisposeManagedResources() { }
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("void DisposeManagedResources"));
        Assert.That(generated, Does.Contain("DisposeUnmanagedResources"));
    }

    [Test]
    public async Task Generator_auto_removes_async_when_IAsyncDisposable_unavailable()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(
            source, ReferenceAssemblies.NetFramework.Net40.Default, LanguageVersion.CSharp5);

        Assert.That(generated, Does.Not.Contain("IAsyncDisposable"));
        Assert.That(generated, Does.Not.Contain("DisposeAsync"));
    }

    [Test]
    public async Task Generator_includes_async_when_IAsyncDisposable_available()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source, ReferenceAssemblies.Net.Net80);

        Assert.That(generated, Does.Contain("IAsyncDisposable"));
        Assert.That(generated, Does.Contain("DisposeAsync"));
    }

    [Test]
    public async Task Generator_includes_async_on_netstandard20_with_bcl_async_interfaces()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var referenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard20.WithPackages(
            [new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "8.0.0")]);

        var generated = await RunGeneratorAndGetOutput(source, referenceAssemblies, LanguageVersion.CSharp7_3);

        Assert.That(generated, Does.Contain("IAsyncDisposable"));
        Assert.That(generated, Does.Contain("DisposeAsync"));
    }

    [Test]
    public async Task Generator_includes_async_on_net462_with_bcl_async_interfaces()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var referenceAssemblies = ReferenceAssemblies.NetFramework.Net462.Default.WithPackages(
            [new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "8.0.0")]);

        var generated = await RunGeneratorAndGetOutput(source, referenceAssemblies, LanguageVersion.CSharp7_3);

        Assert.That(generated, Does.Contain("IAsyncDisposable"));
        Assert.That(generated, Does.Contain("DisposeAsync"));
    }

    [Test]
    public async Task Generator_compiles_on_netstandard13()
    {
        var test = new CSharpSourceGeneratorVerifier.Test
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard13,
            LanguageVersion = LanguageVersion.CSharp7_3,
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
",
        };

        await test.RunAsync();
    }

    [Test]
    public async Task Generator_compiles_on_netstandard20()
    {
        var test = new CSharpSourceGeneratorVerifier.Test
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard20,
            LanguageVersion = LanguageVersion.CSharp7_3,
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
",
        };

        await test.RunAsync();
    }

    [Test]
    public async Task Generator_compiles_on_netstandard21()
    {
        var test = new CSharpSourceGeneratorVerifier.Test
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            LanguageVersion = LanguageVersion.CSharp9,
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestCode = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
",
        };

        await test.RunAsync();
    }

    [Test]
    public async Task Generator_uses_file_scoped_namespace_for_CSharp10_and_above()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(source, languageVersion: LanguageVersion.CSharp12);

        Assert.That(generated, Does.Contain("namespace TestNamespace;"));
    }

    [Test]
    public async Task Generator_uses_block_namespace_for_CSharp9()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(
            source, ReferenceAssemblies.NetStandard.NetStandard21, LanguageVersion.CSharp9);

        Assert.That(generated, Does.Not.Contain("namespace TestNamespace;"));
        Assert.That(generated, Does.Match(@"namespace TestNamespace\s*\{"));
    }

    [Test]
    public async Task Generator_removes_nullable_directives_for_CSharp7_3()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(
            source, ReferenceAssemblies.NetStandard.NetStandard20, LanguageVersion.CSharp7_3);

        Assert.That(generated, Does.Not.Contain("#nullable"));
    }

    [Test]
    public async Task Generator_downgrades_syntax_for_CSharp5()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var generated = await RunGeneratorAndGetOutput(
            source, ReferenceAssemblies.NetFramework.Net40.Default, LanguageVersion.CSharp5);

        Assert.That(generated, Does.Not.Contain("#nullable"));
        Assert.That(generated, Does.Not.Contain("namespace TestNamespace;"));
    }

    [Test]
    public async Task Generator_handles_global_namespace()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

[DisposableMixin]
public partial class TestClass
{
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Not.Contain("namespace"));
        Assert.That(generated, Does.Contain("partial class TestClass"));
    }

    [Test]
    public async Task Generator_wraps_in_partial_struct_for_struct_parent()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public partial struct ParentStruct
    {
        [DisposableMixin]
        public partial class TestClass
        {
        }
    }
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("partial struct ParentStruct"));
    }

    [Test]
    public async Task Generator_wraps_in_partial_record_for_record_parent()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public partial record ParentRecord
    {
        [DisposableMixin]
        public partial class TestClass
        {
        }
    }
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("partial record ParentRecord"));
    }

    [Test]
    public async Task Generator_wraps_in_partial_record_struct_for_record_struct_parent()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public partial record struct ParentRecordStruct
    {
        [DisposableMixin]
        public partial class TestClass
        {
        }
    }
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("partial record struct ParentRecordStruct"));
    }

    [Test]
    public async Task Generator_wraps_in_partial_interface_for_interface_parent()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public partial interface IParentInterface
    {
        [DisposableMixin]
        public partial class TestClass
        {
        }
    }
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("partial interface IParentInterface"));
    }

    [Test]
    public async Task Generator_wraps_deeply_nested_class()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    public partial class Outer
    {
        public partial struct Middle
        {
            [DisposableMixin]
            public partial class Inner
            {
            }
        }
    }
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("partial class Outer"));
        Assert.That(generated, Does.Contain("partial struct Middle"));
        Assert.That(generated, Does.Contain("partial class Inner"));
    }

    [Test]
    public async Task Generator_reads_named_attribute_arguments()
    {
        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin(Options = DisposableMixinOptions.WithProperty)]
    public partial class TestClass
    {
    }
}
";
        var generated = await RunGeneratorAndGetOutput(source);

        Assert.That(generated, Does.Contain("IsDisposed"));
        Assert.That(generated, Does.Not.Contain("event"));
    }

    [Test]
    public async Task Generator_declares_EmbeddedAttribute_without_any_attributes()
    {
        // Every attribute that could sit on this declaration is AllowMultiple = false, so each one
        // is a CS0579 waiting for a co-generator that declares the other partial half of the type.
        // A [Conditional] would additionally un-embed every [Embedded] marker in the consuming
        // project, because the attribute applications are then never written to metadata.
        var source = @"
namespace TestNamespace
{
}
";

        var run = await RunGenerator([source]);

        var embeddedAttribute = FindDeclaredType(GetAttributesSyntaxTree(run), "EmbeddedAttribute");

        Assert.That(run.Errors, Is.Empty);
        Assert.That(embeddedAttribute, Is.Not.Null);
        Assert.That(embeddedAttribute!.AttributeLists, Is.Empty);
    }

    [Test]
    public async Task Generator_coexists_with_a_foreign_EmbeddedAttribute_declaration()
    {
        // The shape PolySharp emits. Both halves merge into one type and attributes union across
        // partials, so an AllowMultiple = false attribute on both sides surfaces as CS0579.
        var foreignHalf = @"
namespace Microsoft.CodeAnalysis
{
    [global::System.AttributeUsage(global::System.AttributeTargets.All)]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal sealed partial class EmbeddedAttribute : global::System.Attribute
    {
    }
}
";

        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var run = await RunGenerator([source, foreignHalf]);

        Assert.That(run.Errors, Is.Empty);
        Assert.That(GetGeneratedHintNames(run), Does.Contain("TestClass.g.cs"));
    }

    [Test]
    public async Task Generator_omits_only_EmbeddedAttribute_for_NO_EMBEDDED_ATTRIBUTE()
    {
        var ownEmbeddedAttribute = @"
namespace Microsoft.CodeAnalysis
{
    internal sealed partial class EmbeddedAttribute : global::System.Attribute
    {
    }
}
";

        var source = @"
using Josupeit.Practices.Mixins.Disposable;

namespace TestNamespace
{
    [DisposableMixin]
    public partial class TestClass
    {
    }
}
";

        var run = await RunGenerator(
            [source, ownEmbeddedAttribute],
            preprocessorSymbols: [NO_EMBEDDED_ATTRIBUTE_SYMBOL]);

        var attributes = GetAttributesSyntaxTree(run);

        Assert.That(run.Errors, Is.Empty);
        Assert.That(FindDeclaredType(attributes, "EmbeddedAttribute"), Is.Null);
        Assert.That(FindDeclaredType(attributes, "DisposableMixinAttribute"), Is.Not.Null);
        Assert.That(FindDeclaredType(attributes, "DisposableMixinOptions"), Is.Not.Null);
        Assert.That(GetGeneratedHintNames(run), Does.Contain("TestClass.g.cs"));
    }

    [Test]
    public async Task Generator_requires_an_own_EmbeddedAttribute_for_NO_EMBEDDED_ATTRIBUTE()
    {
        // The mixin's own attributes still wear [Embedded], so opting out of the declaration
        // without supplying a replacement leaves them unresolvable. Documented in the README.
        var source = @"
namespace TestNamespace
{
}
";

        var run = await RunGenerator([source], preprocessorSymbols: [NO_EMBEDDED_ATTRIBUTE_SYMBOL]);

        Assert.That(run.Errors.Select(x => x.Id), Is.All.EqualTo("CS0234"));
        Assert.That(run.Errors, Is.Not.Empty);
    }

    [Test]
    public async Task Generator_omits_all_attributes_for_NO_ATTRIBUTES()
    {
        await AssertNoTypesAreDeclared(NO_ATTRIBUTES_SYMBOL);
    }

    [Test]
    public async Task Generator_omits_all_attributes_for_both_opt_out_symbols()
    {
        await AssertNoTypesAreDeclared(NO_ATTRIBUTES_SYMBOL, NO_EMBEDDED_ATTRIBUTE_SYMBOL);
    }

    private static async Task AssertNoTypesAreDeclared(params string[] preprocessorSymbols)
    {
        var source = @"
namespace TestNamespace
{
}
";

        var run = await RunGenerator([source], preprocessorSymbols: preprocessorSymbols);

        var declaredTypes = GetAttributesSyntaxTree(run)
            .GetRoot()
            .DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>();

        Assert.That(run.Errors, Is.Empty);
        Assert.That(declaredTypes, Is.Empty);
    }

    private static async Task<string> RunGeneratorAndGetOutput(string source, ReferenceAssemblies? referenceAssemblies = null, LanguageVersion languageVersion = LanguageVersion.CSharp12)
    {
        var run = await RunGenerator([source], referenceAssemblies, languageVersion);

        Assert.That(run.Errors, Is.Empty, "Compilation should succeed without errors");

        var generatedSource = run.Result.Results[0].GeneratedSources.Single(s => s.HintName != ATTRIBUTES_HINT_NAME);

        return generatedSource.SourceText.ToString();
    }

    private static async Task<GeneratorRun> RunGenerator(
        string[] sources,
        ReferenceAssemblies? referenceAssemblies = null,
        LanguageVersion languageVersion = LanguageVersion.CSharp12,
        string[]? preprocessorSymbols = null)
    {
        referenceAssemblies ??= ReferenceAssemblies.Net.Net80;
        var references = await referenceAssemblies.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);

        var parseOptions = new CSharpParseOptions(languageVersion, preprocessorSymbols: preprocessorSymbols);

        var syntaxTrees = sources
            .Select(x => CSharpSyntaxTree.ParseText(x, parseOptions))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver
            .Create(new DisposableSourceGenerator())
            .WithUpdatedParseOptions(parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var errors = outputCompilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        return new GeneratorRun(driver.GetRunResult(), errors);
    }

    private static SyntaxTree GetAttributesSyntaxTree(GeneratorRun run)
    {
        return run.Result.Results[0].GeneratedSources.Single(x => x.HintName == ATTRIBUTES_HINT_NAME).SyntaxTree;
    }

    private static IEnumerable<string> GetGeneratedHintNames(GeneratorRun run)
    {
        return run.Result.Results[0].GeneratedSources.Select(x => x.HintName);
    }

    private static BaseTypeDeclarationSyntax? FindDeclaredType(SyntaxTree tree, string name)
    {
        return tree
            .GetRoot()
            .DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .SingleOrDefault(x => x.Identifier.ValueText == name);
    }

    private sealed record GeneratorRun(GeneratorDriverRunResult Result, ImmutableArray<Diagnostic> Errors);
}
