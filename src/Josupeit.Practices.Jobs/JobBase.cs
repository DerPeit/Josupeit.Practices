using System;
using System.Threading;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class is a base class for all kinds of jobs.
/// </summary>
[PublicAPI]
public abstract class JobBase
{
    private readonly CancellationTokenSource _cancellation = new();

    /// <summary>
    /// Occurs when any property of the job changed.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Gets the job identifier.
    /// </summary>
    /// <remarks>
    /// The identifier is a random value which is assigned when the job is created.
    /// </remarks>
    /// <value>The identifier.</value>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the exception of a faulted job.
    /// </summary>
    /// <value>The exception.</value>
    public Exception? Exception { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this job is canceled.
    /// </summary>
    /// <value><c>true</c> if this job is canceled; otherwise, <c>false</c>.</value>
    public bool IsCanceled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this job is completed.
    /// </summary>
    /// <value><c>true</c> if this job is completed; otherwise, <c>false</c>.</value>
    /// <remarks>A job is completed when it is canceled, faulted or completed successfully.</remarks>
    public bool IsCompleted { get; private protected set; }

    /// <summary>
    /// Gets a value indicating whether this job is completed successfully.
    /// </summary>
    /// <value><c>true</c> if this job is completed successfully; otherwise, <c>false</c>.</value>
    public bool IsCompletedSuccessfully { get; private protected set; }

    /// <summary>
    /// Gets a value indicating whether this job is executing.
    /// </summary>
    /// <value><c>true</c> if this job is executing; otherwise, <c>false</c>.</value>
    public bool IsExecuting { get; private protected set; }

    /// <summary>
    /// Gets a value indicating whether this job is faulted.
    /// </summary>
    /// <value><c>true</c> if this job is faulted; otherwise, <c>false</c>.</value>
    public bool IsFaulted { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this job is pending (i.e. waiting for execution).
    /// </summary>
    /// <value><c>true</c> if this job is pending; otherwise, <c>false</c>.</value>
    public bool IsPending { get; private set; }

    /// <summary>
    /// Gets a token that is canceled when the job is canceled.
    /// </summary>
    /// <value>The cancellation token.</value>
    protected CancellationToken CancellationToken => _cancellation.Token;

    /// <summary>
    /// Starts this job.
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Cancels this job.
    /// </summary>
    public virtual void Cancel()
    {
        ThrowIfCannotCancel();

        if (_cancellation.IsCancellationRequested)
            return;

        ThreadPool.UnsafeQueueUserWorkItem(
            _ =>
            {
                try
                {
                    _cancellation.Cancel();
                }
                catch
                {
                    // Any exceptions are being purposely discarded.
                }
            },
            state: null);
    }

    /// <summary>
    /// Invokes the <see cref="Changed"/> event.
    /// </summary>
    protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Sets the <see cref="IsPending" /> property to <c>true</c> and invokes the <see cref="Changed" /> event.
    /// </summary>
    protected void NotifyPending()
    {
        IsPending = true;

        OnChanged();
    }

    /// <summary>
    /// Sets the <see cref="IsPending" /> property to <c>false</c>, the <see cref="IsExecuting" /> property to
    /// <c>true</c> and invokes the <see cref="Changed" /> event.
    /// </summary>
    protected void NotifyExecuting()
    {
        IsPending = false;
        IsExecuting = true;

        OnChanged();
    }

    /// <summary>
    /// Sets the <see cref="IsExecuting" /> property to <c>false</c>, the <see cref="IsCompleted" /> and
    /// <see cref="IsFaulted"/> properties to <c>true</c>, sets the <see cref="Exception"/> property and
    /// invokes the <see cref="Changed" /> event.
    /// </summary>
    protected void NotifyFailed(Exception ex)
    {
        Exception = ex;
        IsCompleted = true;
        IsExecuting = false;
        IsFaulted = true;

        OnChanged();
    }

    /// <summary>
    /// Sets the <see cref="IsPending" /> and <see cref="IsExecuting"/> properties to <c>false</c>, the
    /// <see cref="IsCompleted" /> and <see cref="IsCanceled"/> properties to <c>true</c> and invokes
    /// the <see cref="Changed" /> event.
    /// </summary>
    protected void NotifyCanceled()
    {
        IsPending = false;
        IsCompleted = true;
        IsExecuting = false;
        IsCanceled = true;

        OnChanged();
    }

    /// <summary>
    /// Throws if the job currently cannot be canceled.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">The job cannot be canceled because it already ran to completion.</exception>
    protected void ThrowIfCannotCancel()
    {
        if (IsCompleted)
            throw new InvalidOperationException("The job cannot be canceled because it already ran to completion.");
    }

    /// <summary>
    /// Throws if the job currently cannot start.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">The job cannot be started because it is already waiting for execution.</exception>
    /// <exception cref="System.InvalidOperationException">The job cannot be started because it is already executing.</exception>
    /// <exception cref="System.InvalidOperationException">The job cannot be started because it already ran to completion.</exception>
    protected void ThrowIfCannotStart()
    {
        if (IsPending)
            throw new InvalidOperationException("The job cannot be started because it is already waiting for execution.");

        if (IsExecuting)
            throw new InvalidOperationException("The job cannot be started because it is already executing.");

        if (IsCompleted)
            throw new InvalidOperationException("The job cannot be started because it already ran to completion.");
    }
}
