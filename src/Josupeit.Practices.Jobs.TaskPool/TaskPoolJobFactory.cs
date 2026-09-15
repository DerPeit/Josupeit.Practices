// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job factory that creates task pool jobs.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.IJobFactory" />.
/// </summary>
/// <seealso cref="Josupeit.Practices.Jobs.IJobFactory" />
[PublicAPI]
public class TaskPoolJobFactory : IJobFactory
{
    /// <inheritdoc />
    public virtual IJob Create(Func<Task> startFunc)
    {
        return new TaskPoolJob(_ => startFunc());
    }

    /// <inheritdoc />
    public virtual IJob Create(Func<CancellationToken, Task> startFunc)
    {
        return new TaskPoolJob(startFunc);
    }

    /// <inheritdoc />
    public virtual IJobWithProgress<TProgress> Create<TProgress>(Func<IProgress<TProgress>, Task> startFunc)
    {
        return new TaskPoolJobWithProgress<TProgress>((progress, _) => startFunc(progress));
    }

    /// <inheritdoc />
    public virtual IJobWithProgress<TProgress> Create<TProgress>(Func<IProgress<TProgress>, CancellationToken, Task> startFunc)
    {
        return new TaskPoolJobWithProgress<TProgress>(startFunc);
    }

    /// <inheritdoc />
    public virtual IJobWithResult<TResult> Create<TResult>(Func<Task<TResult>> startFunc)
    {
        return new TaskPoolJobWithResult<TResult>(_ => startFunc());
    }

    /// <inheritdoc />
    public virtual IJobWithResult<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> startFunc)
    {
        return new TaskPoolJobWithResult<TResult>(startFunc);
    }

    /// <inheritdoc />
    public virtual IJobWithProgressAndResult<TProgress, TResult> Create<TProgress, TResult>(Func<IProgress<TProgress>, Task<TResult>> startFunc)
    {
        return new TaskPoolJobWithProgressAndResult<TProgress, TResult>((progress, _) => startFunc(progress));
    }

    /// <inheritdoc />
    public virtual IJobWithProgressAndResult<TProgress, TResult> Create<TProgress, TResult>(Func<IProgress<TProgress>, CancellationToken, Task<TResult>> startFunc)
    {
        return new TaskPoolJobWithProgressAndResult<TProgress, TResult>(startFunc);
    }
}
