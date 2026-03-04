using System;
using JetBrains.Annotations;

namespace Josupeit.Practices.Services;

/// <summary>
/// This interface represents a service that processes tasks in background.
/// </summary>
[PublicAPI]
public interface IService
{
    /// <summary>
    /// Gets the current state of this instance, i.e. weather it is running,
    /// paused or stopped.
    /// </summary>
    /// <value>The current state of this instance.</value>
    ServiceState State { get; }

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    /// <exception cref="NotSupportedException">This instance cannot be paused.</exception>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be paused.</exception>
    void Pause();

    /// <summary>
    /// Resumes this instance.
    /// </summary>
    /// <exception cref="NotSupportedException">This instance cannot be paused and thus, it cannot be resumed.</exception>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be resumed.</exception>
    void Resume();

    /// <summary>
    /// Starts this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be started.</exception>
    void Start();

    /// <summary>
    /// Stops this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be stopped.</exception>
    void Stop();
}