using Microsoft.CodeAnalysis;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal interface IRewritePipeline
{
    SyntaxNode Run(SyntaxNode node);
}