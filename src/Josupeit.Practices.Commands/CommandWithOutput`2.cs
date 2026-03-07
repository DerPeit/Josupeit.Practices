// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This class is a base class and represents a command with return value.
/// </summary>
/// <typeparam name="TInput">The type of the input parameter.</typeparam>
/// <typeparam name="TOutput">The type of the result.</typeparam>
/// <seealso cref="ICommandWithOutput{TInput,TOutput}" />
[PublicAPI]
public abstract class CommandWithOutput<TInput, TOutput> : Executable, ICommandWithOutput<TInput, TOutput>
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>A result.</returns>
    /// <remarks>
    /// Override this method with a synchronous implementation but omit a call to the
    /// base implementation. The base implementation is intended to call the asynchronous
    /// overload and vice versa to ensure that commands can always be called synchronously
    /// or asynchronously regardless of the implementation. Override the asynchronous
    /// method <see cref="ExecuteAsync(TInput, CancellationToken)"/> only, if the command
    /// also has an equal asynchronous implementation.
    /// </remarks>
    public virtual TOutput Execute(TInput input)
    {
        using (CreateExecuteScope())
        {
            return ExecuteAsync(input, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    public Task<TOutput> ExecuteAsync(TInput input) => ExecuteAsync(input, CancellationToken.None);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task{TOutput}"/> that represents the asynchronous progress encapsulating a result.</returns>
    /// <remarks>
    /// Override this method with an asynchronous implementation but omit a call to the
    /// base implementation. The base implementation is intended to call the synchronous
    /// overload and vice versa to ensure that commands can always be called synchronously
    /// or asynchronously regardless of the implementation. Override the synchronous
    /// method <see cref="Execute"/> only, if the command also has an equal synchronous
    /// implementation.
    /// </remarks>
    public virtual Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken)
    {
        using (CreateExecuteScope())
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TOutput>(cancellationToken);

            try
            {
                var result = Execute(input);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return Task.FromException<TOutput>(ex);
            }
        }
    }
}
