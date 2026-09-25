// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This interface represents a parameterless command with return value.
/// </summary>
/// <typeparam name="TOutput">The type of the result.</typeparam>
[PublicAPI]
public interface ICommandWithOutput<TOutput>
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <returns>A result.</returns>
    TOutput Execute();

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    /// <remarks>By contract equivalent to calling <see cref="ExecuteAsync(CancellationToken)"/> with <see cref="CancellationToken.None"/>.</remarks>
    Task<TOutput> ExecuteAsync();

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    Task<TOutput> ExecuteAsync(CancellationToken cancellationToken);
}
