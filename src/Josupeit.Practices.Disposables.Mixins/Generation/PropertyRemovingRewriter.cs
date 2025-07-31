using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Josupeit.Practices.Disposables.Mixins.Generation;

internal sealed class PropertyRemovingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        return null;
    }
}