using Microsoft.CodeAnalysis.CSharp;

namespace Josupeit.Practices.Disposables.Mixins.Analysis;

internal sealed record SemanticTarget(
    LanguageVersion LanguageVersion,
    MixinOptions Options,
    TargetType Type,
    TargetTypeProperties TypeProperties);