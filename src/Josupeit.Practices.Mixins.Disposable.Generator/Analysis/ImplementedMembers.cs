// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

[Flags]
internal enum ImplementedMembers
{
    None = 0,
    DisposeManagedResources = 1,
    DisposeManagedResourcesAsync = 2,
    DisposeUnmanagedResources = 4,
}
