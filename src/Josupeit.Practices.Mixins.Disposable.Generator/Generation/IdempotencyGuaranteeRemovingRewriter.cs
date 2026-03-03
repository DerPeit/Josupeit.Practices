using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class IdempotencyGuaranteeRemovingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        return null;
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Identifier.Text == "ThrowIfDisposed")
            return null;

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitBlock(BlockSyntax node)
    {
        if (!node.HasLeadingTrivia)
            return base.VisitBlock(node);

        if (node.GetLeadingTrivia().ToString().Trim() == "// MARKER: Idempotence")
            return null;

        return base.VisitBlock(node);
    }
}