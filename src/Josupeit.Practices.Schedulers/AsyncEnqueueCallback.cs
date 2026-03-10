// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// Represents a asynchronous enqueue callback.
/// </summary>
/// <param name="scheduled">The item to be scheduled.</param>
/// <param name="enqueueCancellationToken">The enqueue cancellation token.</param>
/// <returns>A task.</returns>
[PublicAPI]
public delegate Task AsyncEnqueueCallback(IScheduled scheduled, CancellationToken enqueueCancellationToken);
