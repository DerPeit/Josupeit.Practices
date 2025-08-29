using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class EmptyTryBlockRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitTryStatement(TryStatementSyntax node)
    {
        var @base = base.VisitTryStatement(node);

        if (@base is not TryStatementSyntax @try)
            return @base;

        if (@try.Block.Statements.Count == 0)
        {
            @base = @try.Finally?.Block ?? Block();
            @base = @base.WithLeadingTrivia(Comment("// MARKER: Rewritten try block"));
        }
        else if (@try.Finally?.Block.Statements.Count == 0)
        {
            @base = @try.Block;
            @base = @base.WithLeadingTrivia(Comment("// MARKER: Rewritten try block"));
        }

        return @base;
    }
}