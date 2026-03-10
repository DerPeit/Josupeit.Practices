// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class AsyncDisposableRemovingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitSimpleBaseType(SimpleBaseTypeSyntax node)
    {
        if (node.Type.WithoutTrivia().IsEquivalentTo(ParseTypeName("global::System.IAsyncDisposable")))
            return null;

        return base.VisitSimpleBaseType(node);
    }

    public override SyntaxNode VisitBaseList(BaseListSyntax node)
    {
        return node.Update(VisitToken(node.ColonToken), SeparatedList(VisitList(node.Types).Where(x => x is not null)));
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Identifier.Text is "DisposeAsync" or "DisposeManagedResourcesAsync" or "DisposeAsyncCore")
            return null;

        return base.VisitMethodDeclaration(node);
    }
}
