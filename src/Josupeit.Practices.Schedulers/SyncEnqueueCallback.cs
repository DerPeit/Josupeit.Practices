// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// Represents a synchronous enqueue callback.
/// </summary>
/// <param name="scheduled">The item to be scheduled.</param>
[PublicAPI]
public delegate void SyncEnqueueCallback(IScheduled scheduled);
