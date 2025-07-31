using Microsoft.CodeAnalysis;

namespace Josupeit.Practices.Disposables.Mixins.Generation;

internal interface IRewritePipeline
{
    SyntaxNode Run(SyntaxNode node);
}