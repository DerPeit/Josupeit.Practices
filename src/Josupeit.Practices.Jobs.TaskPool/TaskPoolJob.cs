// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job that executes on the task pool.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.Job" />.
/// </summary>
/// <seealso cref="Josupeit.Practices.Jobs.Job" />
[PublicAPI]
public class TaskPoolJob : Job
{
    private readonly Func<CancellationToken, Task> _startFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolJob"/> class.
    /// </summary>
    /// <param name="startFunc">The start function that is invoked when the job is started.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="startFunc"/> must not be <c>null</c>.</exception>
    public TaskPoolJob(Func<CancellationToken, Task> startFunc)
    {
        _startFunc = startFunc ?? throw new ArgumentNullException(nameof(startFunc));
    }

    /// <inheritdoc />
    public override async void Start()
    {
        ThrowIfCannotStart();

        NotifyPending();

        await Task.Yield();

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
