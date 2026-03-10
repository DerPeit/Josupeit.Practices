// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Microsoft.CodeAnalysis;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal interface IRewritePipeline
{
    SyntaxNode Run(SyntaxNode node);
}
