using System.Threading;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// This class represents a simple scheduler that immediately executes a scheduled item on the task pool.<para />
/// Implements the <see cref="Josupeit.Practices.Schedulers.Scheduler" />
/// </summary>
/// <seealso cref="Josupeit.Practices.Schedulers.Scheduler" />
public class TaskPoolScheduler : Scheduler
{
    /// <inheritdoc />
    protected override void Enqueue(IScheduled scheduled)
    {
        _ = scheduled.ExecuteAsync(CancellationToken.None);
    }
}