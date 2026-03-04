using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Josupeit.Practices.Schedulers;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job that executes on a particular scheduler.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.Job" />
/// </summary>
/// <seealso cref="Josupeit.Practices.Jobs.Job" />
[PublicAPI]
public class SchedulerJob : Job
{
    private readonly IScheduler _scheduler;
    private readonly Func<CancellationToken, Task> _startFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerJob"/> class.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="scheduler"/> must not be <c>null</c>.</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="startFunc"/> must not be <c>null</c>.</exception>
    public SchedulerJob(IScheduler scheduler, Func<CancellationToken, Task> startFunc)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _startFunc = startFunc ?? throw new ArgumentNullException(nameof(startFunc));
    }

    /// <inheritdoc />
    public override void Start()
    {
        ThrowIfCannotStart();

        NotifyPending();

        _ = _scheduler.ScheduleAsync(StartCore);
    }

    /// <summary>
    /// This method is being called when the scheduler picks up and attempts to start this job.
    /// </summary>
    protected virtual async Task StartCore()
    {
        NotifyExecuting();

        try
        {
            await _startFunc(CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (CancellationToken.IsCancellationRequested)
        {
            NotifyCancelled();

            return;
        }
        catch (Exception ex)
        {
            NotifyFailed(ex);

            return;
        }

        NotifySucceeded();
    }
}