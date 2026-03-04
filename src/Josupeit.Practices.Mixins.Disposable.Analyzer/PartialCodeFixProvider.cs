using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;

namespace Josupeit.Practices.Mixins.Disposable.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PartialCodeFixProvider))]
[Shared]
public sealed class PartialCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
        Diagnostics.TargetMustBePartial.Id,
        Diagnostics.EnclosingTypeMustBePartial.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var typeDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (typeDeclaration == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add 'partial' keyword",
                createChangedDocument: c => AddPartialModifierAsync(context.Document, typeDeclaration, c),
                equivalenceKey: "AddPartialModifier"),
            diagnostic);
    }

    private static async Task<Document> AddPartialModifierAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
    {
        var newClassDeclaration = typeDeclaration.WithModifiers(
            typeDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword)));
     
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(typeDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}
