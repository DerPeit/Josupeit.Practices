// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Services;

/// <summary>
/// This interface represents an async service that processes tasks in background.
/// </summary>
[PublicAPI]
public interface IAsyncService
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
    /// <returns>Task.</returns>
    /// <exception cref="NotSupportedException">This instance cannot be paused.</exception>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be paused.</exception>
    Task PauseAsync();

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    /// <exception cref="NotSupportedException">This instance cannot be paused.</exception>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be paused.</exception>
    Task PauseAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Resumes this instance.
    /// </summary>
    /// <returns>Task.</returns>
    /// <exception cref="NotSupportedException">This instance cannot be paused and thus, it cannot be resumed.</exception>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be resumed.</exception>
    Task ResumeAsync();

    /// <summary>
    /// Resumes this instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    /// <exception cref="NotSupportedException">This instance cannot be paused and thus, it cannot be resumed.</exception>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be resumed.</exception>
    Task ResumeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Starts this instance.
    /// </summary>
    /// <returns>Task.</returns>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be started.</exception>
    Task StartAsync();

    /// <summary>
    /// Starts this instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be started.</exception>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops this instance.
    /// </summary>
    /// <returns>Task.</returns>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be stopped.</exception>
    Task StopAsync();

    /// <summary>
    /// Stops this instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    /// <exception cref="InvalidOperationException">This instance is in a state where it cannot be stopped.</exception>
    Task StopAsync(CancellationToken cancellationToken);
}
