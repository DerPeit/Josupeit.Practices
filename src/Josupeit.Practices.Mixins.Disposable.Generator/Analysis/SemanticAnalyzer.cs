using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal ref struct SemanticAnalyzer(GeneratorAttributeSyntaxContext context)
{
    private readonly TypeDeclarationSyntax _typeSyntax = (TypeDeclarationSyntax)context.TargetNode;
    private readonly INamedTypeSymbol _targetSymbol = (INamedTypeSymbol)context.TargetSymbol;

    private LanguageVersion _languageVersion = LanguageVersion.CSharp5;
    private MixinOptions _mixinOptions;
    private TargetType _targetType = null!;
    private TargetTypeProperties _targetTypeProperties = null!;

    public SemanticTarget Analyze(CancellationToken cancellationToken)
    {
        InferLanguageVersion();
        DisableUnavailableFeatures();
        ConfigureMixin();
        DescribeTarget();
        AnalyzeTarget();

        return new SemanticTarget(
            _languageVersion,
            _mixinOptions, 
            _targetType, 
            _targetTypeProperties);
    }

    private void InferLanguageVersion()
    {
        var firstSyntaxTree = context.SemanticModel.Compilation.SyntaxTrees.FirstOrDefault();

        if (firstSyntaxTree == null)
            return;
            
        var csharpOpts = firstSyntaxTree.Options as CSharpParseOptions;
        
        if (csharpOpts == null)
            return;
        
        _languageVersion = csharpOpts.LanguageVersion;
    }

    private void DisableUnavailableFeatures()
    {
        if (context.SemanticModel.Compilation.GetTypeByMetadataName("System.IAsyncDisposable") == null)
            _mixinOptions |= MixinOptions.WithoutAsyncDisposable;
    }

    private void ConfigureMixin()
    {
        var attributeData = _targetSymbol
            .GetAttributes()
            .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == typeof(DisposableMixinAttribute).FullName);

        if (attributeData == null)
            return;

        var optsOrNull = attributeData.NamedArguments
            .Where(x => x.Key == nameof(DisposableMixinAttribute.Options))
            .Select(x => x.Value.Value is int i ? (DisposableMixinOptions)i : default(DisposableMixinOptions?))
            .FirstOrDefault();

        optsOrNull ??= attributeData.ConstructorArguments
            .Where(x => x.Type?.ToDisplayString() == typeof(DisposableMixinOptions).FullName)
            .Select(x => x.Value is int i ? (DisposableMixinOptions)i : default(DisposableMixinOptions?))
            .FirstOrDefault();

        if (optsOrNull is not { } opts)
            return;

        if (opts.HasFlag(DisposableMixinOptions.WithProperty))
            _mixinOptions |= MixinOptions.WithProperty;

        if (opts.HasFlag(DisposableMixinOptions.WithEvent))
            _mixinOptions |= MixinOptions.WithEvent;

        if (opts.HasFlag(DisposableMixinOptions.WithoutIdempotencyGuarantees))
            _mixinOptions |= MixinOptions.WithoutIdempotencyGuarantees;

        if (opts.HasFlag(DisposableMixinOptions.WithoutAsyncDisposable))
            _mixinOptions |= MixinOptions.WithoutAsyncDisposable;
    }

    private void DescribeTarget()
    {
        var @namespace = _targetSymbol.ContainingNamespace.ToDisplayString();
        var name = _targetSymbol.Name;
        var nameWithConstraints = _targetSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        var type = _typeSyntax switch
        {
            ClassDeclarationSyntax => TypeDiscriminator.Class,
            _ => TypeDiscriminator.Unknown,
        };

        _targetType = new TargetType(@namespace, type, name, nameWithConstraints);
    }

    private void AnalyzeTarget()
    {
        var flags = TypeFlags.None;
        var members = ImplementedMembers.None;

        if (_typeSyntax.Modifiers.Any(SyntaxKind.SealedKeyword))
            flags |= TypeFlags.Sealed;

        if (_targetSymbol.GetMembers("DisposeManagedResources").Any(x => x is IMethodSymbol { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 }))
            members |= ImplementedMembers.DisposeManagedResources;

        if (_targetSymbol.GetMembers("DisposeManagedResourcesAsync").Any(x => x is IMethodSymbol { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 }))
            members |= ImplementedMembers.DisposeManagedResourcesAsync;

        if (_targetSymbol.GetMembers("DisposeUnmanagedResources").Any(x => x is IMethodSymbol { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 }))
            members |= ImplementedMembers.DisposeUnmanagedResources;

        _targetTypeProperties = new TargetTypeProperties(flags, members);
    }
}