// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This class is a bass class represents a command without return value.
/// </summary>
/// <typeparam name="TInput">The type of the input parameter.</typeparam>
/// <seealso cref="ICommand{TInput}" />
[PublicAPI]
public abstract class Command<TInput> : Executable, ICommand<TInput>
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <remarks>
    /// Override this method with a synchronous implementation but omit a call to the
    /// base implementation. The base implementation is intended to call the asynchronous
    /// overload and vice versa to ensure that commands can always be called synchronously
    /// or asynchronously regardless of the implementation. Override the asynchronous
    /// method <see cref="ExecuteAsync(TInput, CancellationToken)"/>
    /// only, if the command also has an equal asynchronous implementation.
    /// </remarks>
    public virtual void Execute(TInput input)
    {
        using (CreateExecuteScope())
        {
            ExecuteAsync(input, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    public Task ExecuteAsync(TInput input) => ExecuteAsync(input, CancellationToken.None);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    /// <remarks>
    /// Override this method with an asynchronous implementation but omit a call to the
    /// base implementation. The base implementation is intended to call the synchronous
    /// overload and vice versa to ensure that commands can always be called synchronously
    /// or asynchronously regardless of the implementation. Override the synchronous
    /// method <see cref="Execute"/> only, if the command also has an equal synchronous
    /// implementation.
    /// </remarks>
    public virtual Task ExecuteAsync(TInput input, CancellationToken cancellationToken)
    {
        using (CreateExecuteScope())
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            try
            {
                Execute(input);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
