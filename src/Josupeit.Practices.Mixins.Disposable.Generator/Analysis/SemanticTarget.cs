using Microsoft.CodeAnalysis.CSharp;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record SemanticTarget(
    LanguageVersion LanguageVersion,
    MixinOptions Options,
    bool IsBrowsableAttributeAvailable,
    TargetType Type,
    TargetTypeProperties TypeProperties);