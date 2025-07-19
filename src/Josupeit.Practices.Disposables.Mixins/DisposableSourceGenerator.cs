using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Josupeit.Practices.Mixins.Disposable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Josupeit.Practices.Disposables.Mixins;

[Generator]
public sealed class DisposableSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static x => x.AddSource("Attributes.g.cs", GetMarkerAttributesSource()));

        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(typeof(DisposableMixinAttribute).FullName!, CanBeTransformed, GetSemanticTarget)
            .Where(x => x != null);

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(targets.Collect()),
            static (context, tuple) => GenerateCode(context, tuple.Left, tuple.Right));
    }

    private static SourceText GetMarkerAttributesSource()
    {
        var self = typeof(DisposableSourceGenerator);
        var stream = self.Assembly.GetManifestResourceStream(self, "Attribute.cs")!;

        return SourceText.From(stream, canBeEmbedded: true);
    }

    private static bool CanBeTransformed(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax { Modifiers: var modifiers }
               && modifiers.Any(SyntaxKind.PartialKeyword);
    }

    private static SemanticTarget GetSemanticTarget(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var classSyntax = (ClassDeclarationSyntax)context.TargetNode;
        var targetSymbol = (INamedTypeSymbol)context.TargetSymbol;
        var compilation = context.SemanticModel.Compilation;
        var behaviorAttribute = targetSymbol.GetAttributes().Single(ad => ad.AttributeClass?.ToDisplayString() == typeof(DisposableMixinAttribute).FullName);

        var typeName = targetSymbol.Name;
        var @namespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var typeNameWithConstraints = targetSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var behavior = DisposableMixinBehavior.Default;

        if (behaviorAttribute.ConstructorArguments is [{ Value: int enumValue }, ..])
        {
            behavior = (DisposableMixinBehavior)enumValue;
        }
        else if (behaviorAttribute.NamedArguments.FirstOrDefault(na => na.Key == nameof(DisposableMixinAttribute.Behavior)).Value.Value is int namedEnumValue)
        {
            behavior = (DisposableMixinBehavior)namedEnumValue;
        }

        if (behavior == DisposableMixinBehavior.Default)
            behavior = compilation.GetTypeByMetadataName(typeof(ValueTask).FullName!) is null
                ? DisposableMixinBehavior.ImplementDisposable 
                : DisposableMixinBehavior.ImplementDisposableAndAsyncDisposable;

        if (!Enum.IsDefined(typeof(DisposableMixinBehavior), behavior))
            throw new NotSupportedException("Configured mixin behavior is unsupported.");

        var disposeUnmanagedMethod = targetSymbol
            .GetMembers("DisposeUnmanagedResources")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(x => x is { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 });

        var disposeManagedSyncMethod = targetSymbol
            .GetMembers("DisposeManagedResources")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(x => x is { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 });

        IMethodSymbol? disposeManagedAsyncMethod = null;

        if (behavior is DisposableMixinBehavior.ImplementAsyncDisposable or DisposableMixinBehavior.ImplementDisposableAndAsyncDisposable)
            disposeManagedAsyncMethod = targetSymbol
                .GetMembers("DisposeManagedResourcesAsync")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(x => x is { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 });

        return new SemanticTarget(
            IsSealed: classSyntax.Modifiers.Any(SyntaxKind.SealedKeyword),
            TypeName: typeName,
            Namespace: @namespace,
            TypeNameWithConstraints: typeNameWithConstraints,
            WantsIDisposable: behavior is DisposableMixinBehavior.ImplementDisposable or DisposableMixinBehavior.ImplementDisposableAndAsyncDisposable,
            WantsIAsyncDisposable: behavior is DisposableMixinBehavior.ImplementAsyncDisposable or DisposableMixinBehavior.ImplementDisposableAndAsyncDisposable,
            HasDisposeManagedSyncCallback: disposeManagedSyncMethod != null,
            HasDisposeManagedAsyncCallback: disposeManagedAsyncMethod != null,
            HasDisposeUnmanagedCallback: disposeUnmanagedMethod != null);
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation, ImmutableArray<SemanticTarget> targets)
    {
        if (targets.IsDefaultOrEmpty)
            return;

        var tool = new GeneratedCodeAttribute(typeof(DisposableSourceGenerator).ToString(), typeof(DisposableSourceGenerator).Assembly.GetName().Version.ToString());

        foreach (var target in targets)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            context.AddSource(target.TypeName + ".g.cs", GenerateCode(compilation, tool, target));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "I prefer the Hungarian notation here, exceptionally.")]
    private static SourceText GenerateCode(Compilation compilation, GeneratedCodeAttribute tool, SemanticTarget target)
    {
        return new GenerateCodeImplementation(compilation, tool, target).GenerateCode();
    }
}

file sealed class GenerateCodeImplementation(Compilation compilation, GeneratedCodeAttribute tool, SemanticTarget target)
{
    private readonly LanguageVersion? _langVersion = (compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions)?.LanguageVersion;

#nullable disable
    private IndentingStringBuilder _sb;
#nullable restore

    public SourceText GenerateCode()
    {
        _sb = new();

        EnableNullableContext();

        using (WriteNamespace())
        {
            using (WriteClass())
            {
                WriteIsDisposedField();
                WriteFinalizer();
                WriteDisposingEvent();
                WriteIsDisposedProperty();
                WritePublicDisposeMethod();
                WriteDisposeAsyncMethod();
                WriteDisposeManagedResourcesMethod();
                WriteDisposeManagedResourcesAsyncMethod();
                WriteDisposeUnmanagedResourcesMethod();
                WriteThrowIfDisposedMethod();
                WritePrivateDisposeMethod();
                WriteDisposeAsyncCoreMethodCsharp5();
                WriteDisposeAsyncCoreMethodCsharp6();
                WriteOnDisposingMethod();
            }
        }

        return SourceText.From(_sb.ToString(), Encoding.UTF8);
    }

    private void EnableNullableContext()
    {
        if (_langVersion < LanguageVersion.CSharp8)
            return;

        _sb.AppendLine("#nullable enable");
        _sb.AppendLine([]);
    }

    private IndentingStringBuilderExtensions.EndBlockAction WriteNamespace()
    {
        _sb.Append("namespace ");
        _sb.Append(target.Namespace);

        if (_langVersion >= LanguageVersion.CSharp10)
        {
            _sb.AppendLine(";");
            _sb.AppendLine([]);

            return default;
        }

        return _sb.AppendBlock();
    }

    private IndentingStringBuilderExtensions.EndBlockAction WriteClass()
    {
        _sb.Append("partial class ");
        _sb.Append(target.TypeNameWithConstraints);

        switch (target)
        {
            case { WantsIDisposable: true, WantsIAsyncDisposable: false }:
                _sb.Append(" : global::System.IDisposable");

                break;

            case { WantsIDisposable: false, WantsIAsyncDisposable: true }:
                _sb.Append(" : global::System.IAsyncDisposable");

                break;

            case { WantsIDisposable: true, WantsIAsyncDisposable: true }:
                _sb.Append(" : global::System.IDisposable, global::System.IAsyncDisposable");

                break;
        }

        return _sb.AppendBlock();
    }

    private void WriteIsDisposedField()
    {
        WriteGeneratedCodeAttribute();
        WriteEditorBrowsableAttribute();
        WriteDebuggerBrowsableAttribute();

        _sb
            .AppendLine("private volatile bool _isDisposed;")
            .AppendLine([]);
    }

    private void WriteFinalizer()
    {
        if (target is { IsSealed: true, HasDisposeUnmanagedCallback: false })
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.Append("~");
        _sb.Append(target.TypeName);
        _sb.AppendLine("()");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("this.Dispose(isDisposing: false);");
        }

        _sb.AppendLine([]);
    }

    private void WriteDisposingEvent()
    {
        WriteGeneratedCodeAttribute();

        _sb.Append("public event global::System.EventHandler");

        if (_langVersion >= LanguageVersion.CSharp8)
            _sb.Append("?");

        _sb.AppendLine(" Disposing;");
        _sb.AppendLine([]);
    }

    private void WriteIsDisposedProperty()
    {
        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.AppendLine("public bool IsDisposed");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("get");

            using (_sb.AppendBlock())
            {
                _sb.AppendLine("return this._isDisposed;");
            }
        }
    }

    private void WritePublicDisposeMethod()
    {
        if (!target.WantsIDisposable)
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.AppendLine("public void Dispose()");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("this.Dispose(isDisposing: true);");
            _sb.AppendLine("global::System.GC.SuppressFinalize(this);");
        }

        _sb.AppendLine([]);
    }

    private void WriteDisposeAsyncMethod()
    {
        if (!target.WantsIAsyncDisposable)
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.AppendLine("public async global::System.Threading.Tasks.ValueTask DisposeAsync()");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("await this.DisposeAsyncCore().ConfigureAwait(false);");
            _sb.AppendLine("global::System.GC.SuppressFinalize(this);");
        }

        _sb.AppendLine([]);
    }

    private void WriteDisposeManagedResourcesMethod()
    {
        if (target.HasDisposeManagedSyncCallback)
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.Append(target.IsSealed || !target.WantsIDisposable ? "partial" : "protected virtual");
        _sb.Append(" void DisposeManagedResources()");

        if (target.IsSealed || !target.WantsIDisposable)
            _sb.AppendLine(";");
        else
        {
            _sb.AppendLine([]);

            using (_sb.AppendBlock())
            {
            }
        }

        _sb.AppendLine([]);
    }

    private void WriteDisposeManagedResourcesAsyncMethod()
    {
        if (target.HasDisposeManagedAsyncCallback || !target.WantsIAsyncDisposable)
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.Append(target.IsSealed ? "private" : "protected virtual");
        _sb.AppendLine(" global::System.Threading.Tasks.ValueTask DisposeManagedResourcesAsync()");

        using (_sb.AppendBlock())
            _sb.AppendLine("return new global::System.Threading.Tasks.ValueTask();");

        _sb.AppendLine([]);
    }

    private void WriteDisposeUnmanagedResourcesMethod()
    {
        if (target.HasDisposeUnmanagedCallback)
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.Append(target.IsSealed ? "partial" : "protected virtual");
        _sb.Append(" void DisposeUnmanagedResources()");

        if (target.IsSealed)
            _sb.AppendLine(";");
        else
        {
            _sb.AppendLine([]);

            using (_sb.AppendBlock())
            {
            }
        }

        _sb.AppendLine([]);
    }

    private void WriteThrowIfDisposedMethod()
    {
        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();

        _sb.Append(target.IsSealed ? "private" : "protected");
        _sb.AppendLine(" void ThrowIfDisposed()");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("if (this._isDisposed)");

            using (_sb.AppendBlock())
                _sb.AppendLine("throw new global::System.ObjectDisposedException(this.GetType().ToString());");
        }

        _sb.AppendLine([]);
    }

    private void WritePrivateDisposeMethod()
    {
        if (target is { WantsIDisposable: false, IsSealed: true, HasDisposeUnmanagedCallback: false })
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();
        WriteEditorBrowsableAttribute();

        _sb.AppendLine("private void Dispose(bool isDisposing)");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("bool isAlreadyDisposed = this._isDisposed;");
            _sb.AppendLine("this._isDisposed = true;");
            _sb.AppendLine([]);
            _sb.AppendLine("if (isAlreadyDisposed)");

            using (_sb.AppendBlock())
                _sb.AppendLine("return;");

            _sb.AppendLine([]);
            _sb.AppendLine("if (!isDisposing)");

            using (_sb.AppendBlock())
            {
                _sb.AppendLine("this.DisposeUnmanagedResources();");
                _sb.AppendLine("return;");
            }

            _sb.AppendLine([]);
            _sb.AppendLine("try");

            using (_sb.AppendBlock())
                _sb.AppendLine("this.OnDisposing();");

            _sb.AppendLine("finally");

            using (_sb.AppendBlock())
            {
                _sb.AppendLine("this.DisposeManagedResources();");
                _sb.AppendLine("this.DisposeUnmanagedResources();");
            }
        }

        _sb.AppendLine([]);
    }

    private void WriteDisposeAsyncCoreMethodCsharp5()
    {
        if (!target.WantsIAsyncDisposable || _langVersion is > LanguageVersion.CSharp5)
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();
        WriteEditorBrowsableAttribute();

        _sb.AppendLine("private async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("bool isAlreadyDisposed = this._isDisposed;");
            _sb.AppendLine("this._isDisposed = true;");
            _sb.AppendLine([]);
            _sb.AppendLine("if (isAlreadyDisposed)");

            using (_sb.AppendBlock())
                _sb.AppendLine("return;");

            _sb.AppendLine([]);
            _sb.Append("global::System.Runtime.ExceptionServices.ExceptionDispatchInfo");

            if (_langVersion >= LanguageVersion.CSharp8)
                _sb.Append("?");

            _sb.AppendLine(" edi = null;");
            _sb.AppendLine([]);

            _sb.AppendLine("try");

            using (_sb.AppendBlock())
                _sb.AppendLine("this.OnDisposing();");

            _sb.AppendLine("catch (global::System.Exception ex)");

            using (_sb.AppendBlock())
            {
                _sb.AppendLine("edi = global::System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);");
            }

            _sb.AppendLine([]);
            _sb.AppendLine("await this.DisposeManagedResourcesAsync().ConfigureAwait(false);");
            _sb.AppendLine("this.DisposeUnmanagedResources();");
            _sb.AppendLine([]);
            _sb.AppendLine("if (edi != null)");

            using (_sb.AppendBlock())
                _sb.AppendLine("edi.Throw();");
        }

        _sb.AppendLine([]);
    }

    private void WriteDisposeAsyncCoreMethodCsharp6()
    {
        if (!target.WantsIAsyncDisposable || _langVersion is null or < LanguageVersion.CSharp6)
            return;

        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();
        WriteEditorBrowsableAttribute();

        _sb.AppendLine("private async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");

        using (_sb.AppendBlock())
        {
            _sb.AppendLine("bool isAlreadyDisposed = this._isDisposed;");
            _sb.AppendLine("this._isDisposed = true;");
            _sb.AppendLine([]);
            _sb.AppendLine("if (isAlreadyDisposed)");

            using (_sb.AppendBlock())
                _sb.AppendLine("return;");

            _sb.AppendLine([]);
            _sb.AppendLine("try");

            using (_sb.AppendBlock())
                _sb.AppendLine("this.OnDisposing();");

            _sb.AppendLine("finally");

            using (_sb.AppendBlock())
            {
                _sb.AppendLine("await this.DisposeManagedResourcesAsync().ConfigureAwait(false);");
                _sb.AppendLine("this.DisposeUnmanagedResources();");
            }
        }

        _sb.AppendLine([]);
    }

    private void WriteOnDisposingMethod()
    {
        WriteGeneratedCodeAttribute();
        WriteDebuggerNonUserCodeAttribute();
        WriteEditorBrowsableAttribute();

        _sb.AppendLine("private void OnDisposing()");

        using (_sb.AppendBlock())
        {
            _sb.Append("global::System.EventHandler");

            if (_langVersion >= LanguageVersion.CSharp8)
                _sb.Append("?");

            _sb.AppendLine(" disposing = this.Disposing;");
            _sb.AppendLine([]);
            _sb.AppendLine("if (disposing != null)");

            using (_sb.AppendBlock())
                _sb.AppendLine("disposing.Invoke(this, global::System.EventArgs.Empty);");
        }

        _sb.AppendLine([]);
    }

    private void WriteGeneratedCodeAttribute()
    {
        _sb
            .Append("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"")
            .Append(tool.Tool)
            .Append("\", \"")
            .Append(tool.Version)
            .AppendLine("\")]");
    }

    private void WriteEditorBrowsableAttribute()
    {
        _sb.AppendLine("[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]");
    }

    private void WriteDebuggerBrowsableAttribute()
    {
        _sb.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
    }

    private void WriteDebuggerNonUserCodeAttribute()
    {
        _sb.AppendLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute]");
    }
}
