// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This interface represents a query.
/// </summary>
/// <typeparam name="TInput">The type of the input parameter.</typeparam>
/// <typeparam name="TOutput">The type of the result.</typeparam>
/// <remarks>
/// Although this interface is almost the same as the <see cref="ICommandWithOutput{TInput,TOutput}"/> interface,
/// implementations of it are not expected to apply any observable state changes, i.e. implementations of the
/// execution methods must be pure.
/// </remarks>
[PublicAPI]
public interface IQuery<in TInput, TOutput>
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>A result.</returns>
    [System.Diagnostics.Contracts.Pure]
    TOutput Execute(TInput input);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    /// <remarks>By contract equivalent to calling <see cref="ExecuteAsync(TInput, CancellationToken)"/> with <see cref="CancellationToken.None"/>.</remarks>
    [System.Diagnostics.Contracts.Pure]
    Task<TOutput> ExecuteAsync(TInput input);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    [System.Diagnostics.Contracts.Pure]
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
}
