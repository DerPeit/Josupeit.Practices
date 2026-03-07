// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Microsoft.CodeAnalysis.CSharp;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record SemanticTarget(
    LanguageVersion LanguageVersion,
    MixinOptions Options,
    bool IsBrowsableAttributeAvailable,
    TargetType Type,
    TargetTypeProperties TypeProperties);
