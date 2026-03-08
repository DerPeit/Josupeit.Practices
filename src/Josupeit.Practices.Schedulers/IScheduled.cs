// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// Represents a scheduled action that an <see cref="IScheduler"/> will execute or decline.
/// </summary>
[PublicAPI]
public interface IScheduled
{
    /// <summary>
    /// Gets a value indicating whether the call-site requested cancellation for the particular scheduled action.
    /// </summary>
    /// <remarks>
    /// If cancellation has been requested before the scheduler started with the execution, the underlying
    /// task was already transitioned to a canceled state. So, scheduler should skip execution altogether
    /// in this case.
    /// </remarks>
    /// <value><c>true</c> if cancellation is requested; otherwise, <c>false</c>.</value>
    bool IsCancellationRequested { get; }

    /// <summary>
    /// Declines execution of the particular scheduled action.
    /// </summary>
    /// <remarks>
    /// Schedulers should decide whether they execute or decline a scheduled action. Declining will
    /// transition the underlying task to an error state and results in an exception on the call-site.
    /// </remarks>
    void Decline();

    /// <summary>
    /// Executes the particular scheduled action.
    /// </summary>
    /// <param name="executionCancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    void Execute(CancellationToken executionCancellationToken);

    /// <summary>
    /// Executes the particular scheduled action asynchronously.
    /// </summary>
    /// <param name="executionCancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    Task ExecuteAsync(CancellationToken executionCancellationToken);
}
