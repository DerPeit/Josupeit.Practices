using System;
using System.Diagnostics.Contracts;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class contains extension methods for an <seealso cref="IJob"/>.
/// </summary>
public static class AwaitableExtensions
{
    /// <summary>
    /// Gets the job awaiter.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <returns>A job awaiter.</returns>
    /// <exception cref="System.ArgumentNullException">The <param name="job" /> must not be <c>null</c>.</exception>
    [Pure]
    public static IJobAwaiter GetAwaiter(this IJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));
        
        return new JobAwaiter(job);
    }

    /// <summary>
    /// Waits for the job to complete.
    /// </summary>
    /// <remarks>
    /// This method does not throw if the job is cancelled or faulted.
    /// </remarks>
    /// <param name="job">The job.</param>
    /// <exception cref="System.ArgumentNullException">The <param name="job" /> must not be <c>null</c>.</exception>
    public static void Wait(this IJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        job.GetAwaiter().GetResult();
    }
}