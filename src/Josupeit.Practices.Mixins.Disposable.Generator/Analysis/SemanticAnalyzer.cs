// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Generic;
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
    private bool _isBrowsableAttributeAvailable;
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
            _isBrowsableAttributeAvailable,
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

        _isBrowsableAttributeAvailable = context.SemanticModel.Compilation.GetTypeByMetadataName("System.ComponentModel.BrowsableAttribute") != null;
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
        var @namespace = _targetSymbol.ContainingNamespace switch
        {
            { IsGlobalNamespace: true } => null,
            { } any => any.ToDisplayString(),
        };

        var parents = new Stack<ParentType>();
        var name = _targetSymbol.Name;
        var nameWithConstraints = _targetSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var type = Discriminate(_typeSyntax);

        for (var parentSyntax = _typeSyntax.Parent; parentSyntax is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax or InterfaceDeclarationSyntax; parentSyntax = parentSyntax.Parent)
        {
            var parentSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(parentSyntax)!;

            parents.Push(
                new ParentType(
                    Parent: null,
                    Discriminate((TypeDeclarationSyntax)parentSyntax),
                    parentSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }

        while (parents.Count > 1)
        {
            var parent = parents.Pop();
            var child = parents.Pop();

            child = child with { Parent = parent };
            parents.Push(child);
        }

        _targetType = new TargetType(@namespace, parents.FirstOrDefault(), type, name, nameWithConstraints);

        return;

        static TypeDiscriminator Discriminate(TypeDeclarationSyntax type) => type switch
        {
            ClassDeclarationSyntax => TypeDiscriminator.Class,
            StructDeclarationSyntax => TypeDiscriminator.Struct,
            RecordDeclarationSyntax x when x.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => TypeDiscriminator.StructRecord,
            RecordDeclarationSyntax => TypeDiscriminator.ClassRecord,
            InterfaceDeclarationSyntax => TypeDiscriminator.Interface,
            _ => TypeDiscriminator.Unknown,
        };
    }

    private void AnalyzeTarget()
    {
        var flags = TypeFlags.None;
        var members = ImplementedMembers.None;

        if (_typeSyntax.Modifiers.Any(SyntaxKind.SealedKeyword))
            flags |= TypeFlags.Sealed;

        var baseType = _targetSymbol.BaseType;
        var disposableInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("System.IDisposable")!;
        var asyncDisposableInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("System.IAsyncDisposable");

        while (baseType != null && baseType.SpecialType != SpecialType.System_Object)
        {
            if (baseType.AllInterfaces.Contains(disposableInterface))
            {
                flags |= TypeFlags.HasDisposableBase;
                break;
            }

            if (asyncDisposableInterface != null && baseType.AllInterfaces.Contains(asyncDisposableInterface))
            {
                flags |= TypeFlags.HasDisposableBase;
                break;
            }

            baseType = baseType.BaseType;
        }

        if (_targetSymbol.GetMembers("DisposeManagedResources").Any(x => x is IMethodSymbol { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 }))
            members |= ImplementedMembers.DisposeManagedResources;

        if (_targetSymbol.GetMembers("DisposeManagedResourcesAsync").Any(x => x is IMethodSymbol { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 }))
            members |= ImplementedMembers.DisposeManagedResourcesAsync;

        if (_targetSymbol.GetMembers("DisposeUnmanagedResources").Any(x => x is IMethodSymbol { IsGenericMethod: false, IsPartialDefinition: false, Parameters.Length: 0 }))
            members |= ImplementedMembers.DisposeUnmanagedResources;

        _targetTypeProperties = new TargetTypeProperties(flags, members);
    }
}
