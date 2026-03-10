// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This interface represents a command with return value.
/// </summary>
/// <typeparam name="TInput">The type of the input parameter.</typeparam>
/// <typeparam name="TOutput">The type of the result.</typeparam>
[PublicAPI]
public interface ICommandWithOutput<in TInput, TOutput>
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>A result.</returns>
    TOutput Execute(TInput input);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    Task<TOutput> ExecuteAsync(TInput input);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
}
