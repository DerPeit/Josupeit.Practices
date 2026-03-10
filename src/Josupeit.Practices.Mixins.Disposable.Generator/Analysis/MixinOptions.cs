// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

[Flags]
internal enum MixinOptions
{
    None = 0,
    WithProperty = 1,
    WithEvent = 2,
    WithoutIdempotencyGuarantees = 4,
    WithoutAsyncDisposable = 8,
}
