// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This interface represents a parameterless query.
/// </summary>
/// <typeparam name="TOutput">The type of the result.</typeparam>
/// <remarks>
/// Although this interface is almost the same as the <see cref="ICommandWithOutput{TOutput}"/> interface,
/// implementations of it are not expected to apply any observable state changes, i.e. implementations of the
/// execution methods must be pure.
/// </remarks>
[PublicAPI]
public interface IQuery<TOutput>
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <returns>A result.</returns>
    [System.Diagnostics.Contracts.Pure]
    TOutput Execute();

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    /// <remarks>By contract equivalent to calling <see cref="ExecuteAsync(CancellationToken)"/> with <see cref="CancellationToken.None"/>.</remarks>
    [System.Diagnostics.Contracts.Pure]
    Task<TOutput> ExecuteAsync();

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    [System.Diagnostics.Contracts.Pure]
    Task<TOutput> ExecuteAsync(CancellationToken cancellationToken);
}
