// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// Provides an awaitable object that enables configured awaits on an <see cref="IJob"/>.
/// </summary>
/// <remarks>This type is returned by <see cref="AwaitableExtensions.ConfigureAwait"/>.</remarks>
[PublicAPI]
public readonly struct ConfiguredJobAwaitable
{
    private readonly IJob _job;
    private readonly bool _continueOnCapturedContext;

    internal ConfiguredJobAwaitable(IJob job, bool continueOnCapturedContext)
    {
        _job = job;
        _continueOnCapturedContext = continueOnCapturedContext;
    }

    /// <summary>
    /// Gets the awaiter for this awaitable.
    /// </summary>
    /// <returns>A new job awaiter. Each call allocates a new instance.</returns>
    public IJobAwaiter GetAwaiter() => new JobAwaiter(_job, _continueOnCapturedContext);
}
