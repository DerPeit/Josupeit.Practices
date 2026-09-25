// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Josupeit.Practices.Schedulers;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a factory that creates scheduler jobs.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.IJobFactory" />.
/// </summary>
/// <seealso cref="Josupeit.Practices.Jobs.IJobFactory" />
[PublicAPI]
public class SchedulerJobFactory : IJobFactory
{
    private readonly IScheduler _scheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerJobFactory"/> class.
    /// </summary>
    /// <param name="scheduler">The scheduler on which the jobs will be executed.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="scheduler"/> must not be <c>null</c>.</exception>
    public SchedulerJobFactory(IScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    /// <inheritdoc />
    public virtual IJob Create(Func<Task> startFunc)
    {
        ArgumentNullException.ThrowIfNull(startFunc);

        return new SchedulerJob(_scheduler, _ => startFunc());
    }

    /// <inheritdoc />
    public virtual IJob Create(Func<CancellationToken, Task> startFunc)
    {
        return new SchedulerJob(_scheduler, startFunc);
    }

    /// <inheritdoc />
    public virtual IJobWithProgress<TProgress> Create<TProgress>(Func<IProgress<TProgress>, Task> startFunc)
    {
        ArgumentNullException.ThrowIfNull(startFunc);

        return new SchedulerJobWithProgress<TProgress>(_scheduler, (progress, _) => startFunc(progress));
    }

    /// <inheritdoc />
    public virtual IJobWithProgress<TProgress> Create<TProgress>(Func<IProgress<TProgress>, CancellationToken, Task> startFunc)
    {
        return new SchedulerJobWithProgress<TProgress>(_scheduler, startFunc);
    }

    /// <inheritdoc />
    public virtual IJobWithResult<TResult> Create<TResult>(Func<Task<TResult>> startFunc)
    {
        ArgumentNullException.ThrowIfNull(startFunc);

        return new SchedulerJobWithResult<TResult>(_scheduler, _ => startFunc());
    }

    /// <inheritdoc />
    public virtual IJobWithResult<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> startFunc)
    {
        return new SchedulerJobWithResult<TResult>(_scheduler, startFunc);
    }

    /// <inheritdoc />
    public virtual IJobWithProgressAndResult<TProgress, TResult> Create<TProgress, TResult>(Func<IProgress<TProgress>, Task<TResult>> startFunc)
    {
        ArgumentNullException.ThrowIfNull(startFunc);

        return new SchedulerJobWithProgressAndResult<TProgress, TResult>(_scheduler, (progress, _) => startFunc(progress));
    }

    /// <inheritdoc />
    public virtual IJobWithProgressAndResult<TProgress, TResult> Create<TProgress, TResult>(Func<IProgress<TProgress>, CancellationToken, Task<TResult>> startFunc)
    {
        return new SchedulerJobWithProgressAndResult<TProgress, TResult>(_scheduler, startFunc);
    }
}
