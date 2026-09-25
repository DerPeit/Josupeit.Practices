// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ProtectedVirtualVisibilityCodeFixProvider))]
[Shared]
public sealed class ProtectedVirtualVisibilityCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Diagnostics.MethodShouldBeProtectedVirtual.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make method protected virtual",
                createChangedDocument: c => AdjustVisibilityModifiersAsync(context.Document, methodDeclaration, c),
                equivalenceKey: "MakeMethodProtectedVirtual"),
            diagnostic);
    }

    private static async Task<Document> AdjustVisibilityModifiersAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
    {
        var newMethodDeclaration = methodDeclaration
            .WithModifiers(TokenList(
                methodDeclaration.Modifiers
                    .Where(m => new[] { SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.PrivateKeyword, SyntaxKind.VirtualKeyword }.All(kind => !m.IsKind(kind)))
                    .Prepend(Token(SyntaxKind.VirtualKeyword))
                    .Prepend(Token(SyntaxKind.ProtectedKeyword))))
            .WithTriviaFrom(methodDeclaration);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(methodDeclaration, newMethodDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}
