// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Josupeit.Practices.Schedulers;
using static Josupeit.Practices.Jobs.ScheduleErrorHandler;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job that executes on a particular scheduler.<para />
/// Inherits from <see cref="Josupeit.Practices.Jobs.Job" />.
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
    /// <param name="startFunc">The start function that is invoked when the job is started.</param>
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

        HandleScheduleErrors(
            _scheduler.ScheduleAsync(StartCore, CancellationToken),
            CancellationToken,
            NotifyCanceled,
            NotifyFailed);
    }

    /// <summary>
    /// Invoked by the scheduler to execute this job. Runs the start function and notifies the appropriate
    /// completion state.
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
            NotifyCanceled();

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
