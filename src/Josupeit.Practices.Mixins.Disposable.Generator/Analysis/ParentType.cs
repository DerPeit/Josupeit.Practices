// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record ParentType(
    ParentType? Parent,
    TypeDiscriminator Type,
    string NameWithConstraints);
