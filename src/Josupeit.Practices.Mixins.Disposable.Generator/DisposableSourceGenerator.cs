using System.Collections.Immutable;
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
        return node is ClassDeclarationSyntax { Modifiers: var modifiers } && modifiers.Any(SyntaxKind.PartialKeyword);
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

        foreach (var target in targets)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            context.AddSource(
                target.Type.Name + ".g.cs", 
                new CodeGenerator(target, tool, version).Generate(context.CancellationToken));
        }
    }
}
