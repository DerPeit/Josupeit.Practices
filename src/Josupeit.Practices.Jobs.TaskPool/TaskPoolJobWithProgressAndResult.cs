// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job with a progress and a result that executes on the task pool.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.Job" />.
/// </summary>
/// <typeparam name="TProgress">The type of the progress.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.JobWithProgressAndResult{TProgress,TResult}" />
[PublicAPI]
public class TaskPoolJobWithProgressAndResult<TProgress, TResult> : JobWithProgressAndResult<TProgress, TResult>
{
    private readonly Func<IProgress<TProgress>, CancellationToken, Task<TResult>> _startFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolJobWithProgressAndResult{TProgress, TResult}"/> class.
    /// </summary>
    /// <param name="startFunc">The start function that is invoked when the job is started.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="startFunc"/> must not be <c>null</c>.</exception>
    public TaskPoolJobWithProgressAndResult(Func<IProgress<TProgress>, CancellationToken, Task<TResult>> startFunc)
    {
        _startFunc = startFunc ?? throw new ArgumentNullException(nameof(startFunc));
    }

    /// <inheritdoc />
    public override void Start()
    {
        ThrowIfCannotStart();

        NotifyPending();

        // Task.Run rather than an awaited Task.Yield: the latter posts back to the caller's
        // SynchronizationContext when there is one, so the delegate would run on a UI thread.
        _ = Task.Run(RunAsync);
    }

    private async Task RunAsync()
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
