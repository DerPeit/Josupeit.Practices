// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;

namespace Josupeit.Practices.Jobs;

internal sealed class JobAwaiter(IJob job) : IJobAwaiter
{
    private readonly object _syncRoot = new();
    private readonly IJob _job = job ?? throw new ArgumentNullException(nameof(job));

    private bool _isSubscribedToChangeEvent;

    private event Action? Continuations;

    public bool IsCompleted => _job.IsCompleted;

    public void OnCompleted(Action continuation)
    {
        if (continuation == null)
            throw new ArgumentNullException(nameof(continuation));

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

    private void SubscribeToChangeEvent()
    {
        if (_isSubscribedToChangeEvent)
            return;

        _isSubscribedToChangeEvent = true;
        _job.Changed += RunContinuationsIfCompleted;
    }

    private void RunContinuationsIfCompleted(object sender, EventArgs e)
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
}
