using System;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents a job.
/// </summary>
public interface IJob
{
    /// <summary>
    /// Occurs when any property of the job changed.
    /// </summary>
    event EventHandler Changed;

    /// <summary>
    /// Gets the job identifier.
    /// </summary>
    /// <value>The identifier.</value>
    Guid Id { get; }

    /// <summary>
    /// Gets the exception of a faulted job.
    /// </summary>
    /// <value>The exception.</value>
    Exception? Exception { get; }

    /// <summary>
    /// Gets a value indicating whether this job is cancelled.
    /// </summary>
    /// <value><c>true</c> if this job is cancelled; otherwise, <c>false</c>.</value>
    bool IsCancelled { get; }

    /// <summary>
    /// Gets a value indicating whether this job is completed.
    /// </summary>
    /// <remarks>
    /// A job is completed when it is cancelled, faulted or completed successfully.
    /// </remarks>
    /// <value><c>true</c> if this job is completed; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets a value indicating whether this job is completed successfully.
    /// </summary>
    /// <value><c>true</c> if this job is completed successfully; otherwise, <c>false</c>.</value>
    bool IsCompletedSuccessfully { get; }

    /// <summary>
    /// Gets a value indicating whether this job is executing.
    /// </summary>
    /// <value><c>true</c> if this job is executing; otherwise, <c>false</c>.</value>
    bool IsExecuting { get; }

    /// <summary>
    /// Gets a value indicating whether this job is faulted.
    /// </summary>
    /// <value><c>true</c> if this job is faulted; otherwise, <c>false</c>.</value>
    bool IsFaulted { get; }

    /// <summary>
    /// Gets a value indicating whether this job is pending (i.e. waiting for execution).
    /// </summary>
    /// <value><c>true</c> if this job is pending; otherwise, <c>false</c>.</value>
    bool IsPending { get; }

    /// <summary>
    /// Starts this job.
    /// </summary>
    void Start();

    /// <summary>
    /// Cancels this job.
    /// </summary>
    void Cancel();
}