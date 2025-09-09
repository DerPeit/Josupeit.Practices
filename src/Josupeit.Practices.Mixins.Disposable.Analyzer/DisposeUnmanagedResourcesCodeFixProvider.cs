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

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposeUnmanagedResourcesCodeFixProvider))]
[Shared]
public sealed class DisposeUnmanagedResourcesCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Diagnostics.TargetDoesNotImplementDisposeUnmanagedResources.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Implement 'DisposeUnmanagedResources' method",
                createChangedDocument: c => AddDisposeUnmanagedResourcesMethodAsync(context.Document, classDeclaration, c),
                equivalenceKey: "AddDisposeUnmanagedResourcesMethod"),
            diagnostic);
    }

    private static async Task<Document> AddDisposeUnmanagedResourcesMethodAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var modifiers = classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword) switch
        {
            false => TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.VirtualKeyword)),
            true => TokenList(Token(SyntaxKind.PrivateKeyword)),
        };

        var method = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                "DisposeUnmanagedResources"
            )
            .WithModifiers(modifiers)
            .WithBody(Block())
            .WithLeadingTrivia(ParseLeadingTrivia(
                """
                
                /// <summary>
                /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
                /// </summary>
                
                """
            ));

        var newClassDeclaration = classDeclaration.AddMembers(method);

        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = root!.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}
