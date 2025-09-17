using System;
using System.Collections.Immutable;
using System.Linq;
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
        var stream = self.Assembly.GetManifestResourceStream(self, "Attribute.cs")!;

        return SourceText.From(stream, canBeEmbedded: true);
    }

    private static bool CanBeTransformed(SyntaxNode node, CancellationToken cancellationToken)
    {
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
                    > 1 => string.Concat(nameHint, '-', unchecked((uint)target.GetHashCode()), ".g.cs"),
                    _ => string.Concat(nameHint, ".g.cs"),
                };

                var sourceText = new CodeGenerator(target, tool, version).Generate(context.CancellationToken);

                context.AddSource(sourceTextName, sourceText);
            }
        }
    }
}
