using System;
using System.Threading;

namespace Josupeit.Practices.Jobs;

internal sealed class JobAwaiter : IJobAwaiter
{
    private readonly object _syncRoot = new();
    private readonly IJob _job;

    public JobAwaiter(IJob job)
    {
        _job = job ?? throw new ArgumentNullException(nameof(job));
        _job.Changed += RunContinuationsIfCompleted;
    }

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
        lock (_syncRoot)
            while (!IsCompleted)
                Monitor.Wait(_syncRoot);
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