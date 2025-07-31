using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Disposables.Mixins.Generation;

internal sealed class FinalizerRemovingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        return null;
    }

    public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        var suppressFinalize = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            AliasQualifiedName(IdentifierName(Token(SyntaxKind.GlobalKeyword)), IdentifierName(typeof(GC).FullName!)),
            IdentifierName(nameof(GC.SuppressFinalize)));

        if (node.Expression is InvocationExpressionSyntax { Expression: var expr } && expr.WithoutTrivia().IsEquivalentTo(suppressFinalize))
            return null;

        return base.VisitExpressionStatement(node);
    }
}