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

#pragma warning disable SA1009, SA1111 // Roslyn syntax-factory builder style.

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposeManagedResourcesAsyncCodeFixProvider))]
[Shared]
public sealed class DisposeManagedResourcesAsyncCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Diagnostics.TargetDoesNotImplementDisposeManagedResourcesAsync.Id);

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
                title: "Implement 'DisposeManagedResourcesAsync' method",
                createChangedDocument: c => AddDisposeManagedResourcesAsyncMethodAsync(context.Document, classDeclaration, c),
                equivalenceKey: "AddDisposeManagedResourcesAsyncMethod"),
            diagnostic);
    }

    private static async Task<Document> AddDisposeManagedResourcesAsyncMethodAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var modifiers = classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword) switch
        {
            false => TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.VirtualKeyword)),
            true => TokenList(Token(SyntaxKind.PrivateKeyword)),
        };

        var method = MethodDeclaration(
                IdentifierName("ValueTask"),
                "DisposeManagedResourcesAsync"
            )
            .WithModifiers(modifiers)
            .WithBody(Block(ReturnStatement(
                ObjectCreationExpression(IdentifierName("ValueTask")).WithArgumentList(ArgumentList())
            )))
            .WithLeadingTrivia(ParseLeadingTrivia(
                """
                
                /// <summary>
                /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources asynchronously.
                /// </summary>
                /// <returns>
                /// A <see cref="ValueTask"/> that completes when all asynchronous cleanup operations have finished.
                /// </returns>
                
                """
            ));

        var newClassDeclaration = classDeclaration.AddMembers(method);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(classDeclaration, newClassDeclaration);

        if (newRoot is CompilationUnitSyntax compilationUnit && compilationUnit.Usings.All(u => u.Name.ToString() != "System.Threading.Tasks"))
        {
            var newUsing = UsingDirective(ParseName("System.Threading.Tasks"));
            var usings = compilationUnit.Usings.Add(newUsing);

            usings = new SyntaxList<UsingDirectiveSyntax>(usings.OrderBy(u => u.Name.ToString()));

            compilationUnit = compilationUnit.WithUsings(usings);
            newRoot = compilationUnit;
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
