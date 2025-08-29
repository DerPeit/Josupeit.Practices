using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class MethodRemovingRewriter(string methodName) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Identifier.Text == methodName)
            return null;

        return base.VisitMethodDeclaration(node);
    }
}