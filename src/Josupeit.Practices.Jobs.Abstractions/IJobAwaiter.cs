using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents a job awaiter. It is used by the framework to support awaiting a job.
/// Implements the <see cref="INotifyCompletion" />
/// </summary>
/// <seealso cref="INotifyCompletion" />
[PublicAPI]
public interface IJobAwaiter : INotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether this job is completed.
    /// </summary>
    /// <value><c>true</c> if this job is completed; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }

    /// <summary>
    /// Waits for the job to complete.
    /// </summary>
    /// <remarks>
    /// This method does not throw if the job is canceled or faulted.
    /// </remarks>
    void GetResult();
}