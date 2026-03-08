// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class RewritePipeline : IRewritePipeline
{
    public static readonly RewritePipeline Empty = new();

    private readonly RewritePipeline? _predecessors;
    private readonly CSharpSyntaxRewriter? _rewriter;

    private RewritePipeline()
    {
    }

    public RewritePipeline(RewritePipeline predecessors, CSharpSyntaxRewriter rewriter)
    {
        _predecessors = predecessors;
        _rewriter = rewriter;
    }

    public static RewritePipeline operator |(RewritePipeline predecessors, CSharpSyntaxRewriter rewriter)
        => new(predecessors, rewriter);

    public SyntaxNode Run(SyntaxNode node)
    {
        node = _predecessors?.Run(node) ?? node;
        node = _rewriter?.Visit(node) ?? node;

        return node;
    }
}
