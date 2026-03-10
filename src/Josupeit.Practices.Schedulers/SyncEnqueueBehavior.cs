// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class SyncEnqueueBehavior : IEnqueueBehavior
{
    private readonly SyncEnqueueCallback _syncEnqueueCallback;

    public SyncEnqueueBehavior(SyncEnqueueCallback syncEnqueueCallback)
    {
        _syncEnqueueCallback = syncEnqueueCallback;
    }

    public void Enqueue(IScheduled scheduled, CancellationToken enqueueCancellationToken = default)
    {
        enqueueCancellationToken.ThrowIfCancellationRequested();

        _syncEnqueueCallback(scheduled);
    }

    public Task EnqueueAsync(IScheduled scheduled, CancellationToken enqueueCancellationToken)
    {
        if (enqueueCancellationToken.IsCancellationRequested)
            return Task.FromCanceled(enqueueCancellationToken);

        _syncEnqueueCallback(scheduled);

        return Task.CompletedTask;
    }
}
