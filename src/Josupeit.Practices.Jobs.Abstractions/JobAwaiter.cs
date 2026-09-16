// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;

namespace Josupeit.Practices.Jobs;

internal sealed class JobAwaiter(IJob job, bool continueOnCapturedContext = true) : IJobAwaiter
{
    private readonly IJob _job = job ?? throw new ArgumentNullException(nameof(job));
    private readonly object _syncRoot = new();

    private bool _isSubscribedToChangeEvent;

    private event Action? Continuations;

    public bool IsCompleted => _job.IsCompleted;

    public void GetResult()
    {
        if (IsCompleted)
            return;

        lock (_syncRoot)
        {
            SubscribeToChangeEvent();

            while (!IsCompleted)
                Monitor.Wait(_syncRoot);
        }
    }

    public void OnCompleted(Action continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        continuation = CaptureSynchronizationContext(continuation);

        if (IsCompleted)
        {
            continuation();
            return;
        }

        lock (_syncRoot)
        {
            SubscribeToChangeEvent();

            if (!IsCompleted)
            {
                Continuations += continuation;
                return;
            }
        }

        continuation();
    }

    public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);

    private Action CaptureSynchronizationContext(Action continuation)
    {
        if (!continueOnCapturedContext)
            return continuation;

        var context = SynchronizationContext.Current;

        if (context == null)
            return continuation;

        return () => context.Post(static state => ((Action)state!).Invoke(), continuation);
    }

    private void RunContinuationsIfCompleted(object? sender, EventArgs e)
    {
        if (!IsCompleted)
            return;

        _job.Changed -= RunContinuationsIfCompleted;

        Action? continuations;

        lock (_syncRoot)
        {
            continuations = Continuations;
            Continuations = null;

            Monitor.PulseAll(_syncRoot);
        }

        continuations?.Invoke();
    }

    private void SubscribeToChangeEvent()
    {
        if (_isSubscribedToChangeEvent)
            return;

        _isSubscribedToChangeEvent = true;
        _job.Changed += RunContinuationsIfCompleted;
    }
}
