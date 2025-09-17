using Microsoft.CodeAnalysis.CSharp;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record SemanticTarget(LanguageVersion LanguageVersion, MixinOptions Options, TargetType Type, TargetTypeProperties TypeProperties)
{
    internal string GenerateHintName()
    {
        return Type.GenerateHintName();
    }
}