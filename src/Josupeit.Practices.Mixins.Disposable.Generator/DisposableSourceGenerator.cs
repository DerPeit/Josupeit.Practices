// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Josupeit.Practices.Mixins.Disposable.Generator.Analysis;
using Josupeit.Practices.Mixins.Disposable.Generator.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Josupeit.Practices.Mixins.Disposable.Generator;

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
            static (context, tuple) => GenerateCode(context, tuple.Right));
    }

    private static SourceText GetMarkerAttributesSource()
    {
        var self = typeof(DisposableSourceGenerator);
        var stream = self.Assembly.GetManifestResourceStream(self, "Attributes.cs")!;

        return SourceText.From(stream, canBeEmbedded: true);
    }

    private static bool CanBeTransformed(SyntaxNode node, CancellationToken cancellationToken)
    {
        // Classes only. Records are left to the analyzer, which reports JPMD060: the generated state
        // would take part in a record's synthesized equality.
        if (node is not ClassDeclarationSyntax)
            return false;

        for (var nodeOrParent = node; nodeOrParent is TypeDeclarationSyntax typeDeclaration; nodeOrParent = typeDeclaration.Parent)
            if (!typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                return false;

        return true;
    }

    private static SemanticTarget GetSemanticTarget(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        return new SemanticAnalyzer(context).Analyze(cancellationToken);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<SemanticTarget> targets)
    {
        if (targets.IsDefaultOrEmpty)
            return;

        var tool = typeof(DisposableSourceGenerator).ToString();
        var version = typeof(DisposableSourceGenerator).Assembly.GetName().Version.ToString();

        var eligibleTargetsGroupedByTypeName = targets
            .Where(x => !x.TypeProperties.Flags.HasFlag(TypeFlags.HasDisposableBase))
            .GroupBy(x => x.Type.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in eligibleTargetsGroupedByTypeName)
        {
            var nameHint = kvp.Key;
            var group = kvp.Value;

            foreach (var target in group)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var sourceTextName = group.Count switch
                {
                    > 1 => string.Concat(nameHint, '-', CreateFnv1aHash(GetTypeIdentity(target.Type)), ".g.cs"),
                    _ => string.Concat(nameHint, ".g.cs"),
                };

                var sourceText = new CodeGenerator(target, tool, version).Generate(context.CancellationToken);

                context.AddSource(sourceTextName, sourceText);
            }
        }
    }

    // Namespace, enclosing types from the outermost inwards, and the type's own name with its type
    // parameters: unique within a compilation, and the only thing the hint-name suffix has to tell apart.
    private static string GetTypeIdentity(TargetType type)
    {
        var identity = new StringBuilder();

        if (type.Namespace is not null)
            identity.Append(type.Namespace).Append('.');

        var parents = new Stack<string>();

        for (var parent = type.Parent; parent is not null; parent = parent.Parent)
            parents.Push(parent.NameWithConstraints);

        foreach (var parent in parents)
            identity.Append(parent).Append('+');

        return identity.Append(type.NameWithConstraints).ToString();
    }

    // Hashes a string identically in every process: string.GetHashCode is randomized per process on
    // .NET Core, which would make the generated file name, and with it the consumer's PDB, differ
    // between otherwise identical builds. FNV-1a, 32 bit.
    private static string CreateFnv1aHash(string value)
    {
        var hash = 2166136261u;

        foreach (var c in value)
            hash = (hash ^ c) * 16777619u;

        return hash.ToString("x8", CultureInfo.InvariantCulture);
    }
}
