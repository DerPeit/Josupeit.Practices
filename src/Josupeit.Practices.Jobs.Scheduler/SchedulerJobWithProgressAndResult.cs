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
/// This class represents a job with progress reporting and a result that executes on a particular scheduler.<para />
/// Inherits from <see cref="Josupeit.Practices.Jobs.JobWithProgressAndResult{TProgress, TResult}" />.
/// </summary>
/// <typeparam name="TProgress">The type of the progress.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.JobWithProgressAndResult{TProgress, TResult}" />
[PublicAPI]
public class SchedulerJobWithProgressAndResult<TProgress, TResult> : JobWithProgressAndResult<TProgress, TResult>
{
    private readonly IScheduler _scheduler;
    private readonly Func<IProgress<TProgress>, CancellationToken, Task<TResult>> _startFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerJobWithProgressAndResult{TProgress, TResult}"/> class.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="startFunc">The start function that is invoked when the job is started.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="scheduler"/> must not be <c>null</c>.</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="startFunc"/> must not be <c>null</c>.</exception>
    public SchedulerJobWithProgressAndResult(IScheduler scheduler, Func<IProgress<TProgress>, CancellationToken, Task<TResult>> startFunc)
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
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task StartCore()
    {
        NotifyExecuting();

        var progress = new Progress<TProgress>(NotifyProgress);

        TResult result;

        try
        {
            result = await _startFunc(progress, CancellationToken).ConfigureAwait(false);
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

        NotifySucceeded(result);
    }
}
