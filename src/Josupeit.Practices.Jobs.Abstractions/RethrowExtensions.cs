using System;
using System.Runtime.ExceptionServices;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class contains extension methods for an <seealso cref="IJob"/>.
/// </summary>
public static class RethrowExtensions
{
    /// <summary>
    /// Re-throws the exception, if the particular job is faulted.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <exception cref="System.ArgumentNullException">The <param name="job" /> must not be <c>null</c>.</exception>
    public static void ThrowIfFaulted(this IJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        if (job.IsFaulted)
            ExceptionDispatchInfo.Capture(job.Exception!).Throw();
    }
}