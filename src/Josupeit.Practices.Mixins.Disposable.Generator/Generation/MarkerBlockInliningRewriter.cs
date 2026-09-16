// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class MarkerBlockInliningRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        var statements = node.Statements.SelectMany(statement =>
        {
            if (statement is BlockSyntax { HasLeadingTrivia: true } block && block.GetLeadingTrivia().ToString().Trim().StartsWith("// MARKER:", StringComparison.Ordinal))
                return block.Statements;

            return [statement];
        });

        return node.Update(
            VisitList(node.AttributeLists),
            VisitToken(node.OpenBraceToken),
            VisitList(List(statements)),
            VisitToken(node.CloseBraceToken));
    }
}
