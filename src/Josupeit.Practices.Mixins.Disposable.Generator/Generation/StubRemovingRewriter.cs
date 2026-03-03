using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class StubRemovingRewriter : CSharpSyntaxRewriter
{
    private IList<MethodDeclarationSyntax>? _deleted;

    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        if (_deleted != null)
            return base.Visit(node);

        _deleted = [];

        try
        {
            return base.Visit(node);
        }
        finally
        {
            _deleted = null!;
        }
    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var retain = new List<MemberDeclarationSyntax>(capacity: node.Members.Count);

        foreach (var member in node.Members)
        {
            if (member is not MethodDeclarationSyntax { HasLeadingTrivia: true } method || !method.GetLeadingTrivia().ToString().Contains("// Stub"))
            {
                retain.Add(member);
                continue;
            }

            if (method.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword)))
            {
                _deleted!.Add(method);
                continue;
            }

            retain.Add(member);
        }

        return base.VisitClassDeclaration(node.WithMembers(List(retain)));
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is MemberAccessExpressionSyntax memberAccess && _deleted!.Any(x => x.Identifier.IsEquivalentTo(memberAccess.Name.Identifier)))
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

    public override SyntaxNode? VisitAwaitExpression(AwaitExpressionSyntax node)
    {
        var expression = Visit(node.Expression);

        if (expression == null)
            return null;

        return node.Update(VisitToken(node.AwaitKeyword), (ExpressionSyntax)expression);
    }
}