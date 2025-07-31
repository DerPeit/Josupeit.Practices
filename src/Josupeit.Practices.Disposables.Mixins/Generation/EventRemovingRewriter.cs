using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Josupeit.Practices.Disposables.Mixins.Generation;

internal sealed class EventRemovingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        return base.Visit(node);
    }

    public override SyntaxNode? VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        return null;
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Identifier.Text == "OnDisposing")
            return null;

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "OnDisposing" })
            return null;

        return base.VisitInvocationExpression(node);
    }

    public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        var expression = Visit(node.Expression);

        if (expression == null)
            return null;

        return node.Update(VisitList(node.AttributeLists), (ExpressionSyntax)expression, VisitToken(node.SemicolonToken));
    }
}