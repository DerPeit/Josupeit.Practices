// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class AsyncEnqueueBehavior : IEnqueueBehavior
{
    private readonly AsyncEnqueueCallback _asyncEnqueueCallback;

    public AsyncEnqueueBehavior(AsyncEnqueueCallback asyncEnqueueCallback)
    {
        _asyncEnqueueCallback = asyncEnqueueCallback;
    }

    public void Enqueue(IScheduled scheduled, CancellationToken enqueueCancellationToken = default)
        => EnqueueAsync(scheduled, enqueueCancellationToken).GetAwaiter().GetResult();

    public Task EnqueueAsync(IScheduled scheduled, CancellationToken enqueueCancellationToken)
        => _asyncEnqueueCallback(scheduled, enqueueCancellationToken);
}
