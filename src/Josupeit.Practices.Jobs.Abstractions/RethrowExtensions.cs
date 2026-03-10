// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class contains extension methods for an <seealso cref="IJob"/>.
/// </summary>
[PublicAPI]
public static class RethrowExtensions
{
    /// <summary>
    /// Re-throws the exception, if the particular job is faulted.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="job" /> must not be <c>null</c>.</exception>
    public static void ThrowIfFaulted(this IJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        if (job.IsFaulted)
            ExceptionDispatchInfo.Capture(job.Exception!).Throw();
    }
}
