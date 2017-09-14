using System;
using System.Threading;
using System.Threading.Tasks;
using Josupeit.Practices.Polyfills.Tasks;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This class is a base class and represents a parameterless command without return value.
/// </summary>
/// <seealso cref="ICommand" />
public abstract class Command : Executable, ICommand
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <remarks>
    /// Override this method with a synchronous implementation but omit a call to the
    /// base implementation. The base implementation is intended to call the asynchronous
    /// overload and vice versa to ensure that commands can always be called synchronously
    /// or asynchronously regardless of the implementation. Override the asynchronous
    /// method <see cref="ExecuteAsync(CancellationToken)"/> 
    /// only, if the command also has an equal asynchronous implementation.
    /// </remarks>
    public virtual void Execute()
    {
        using (CreateExecuteScope())
        {
            ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    public Task ExecuteAsync() => ExecuteAsync(CancellationToken.None);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    /// <remarks>
    /// Override this method with an asynchronous implementation but omit a call to the
    /// base implementation. The base implementation is intended to call the synchronous
    /// overload and vice versa to ensure that commands can always be called synchronously
    /// or asynchronously regardless of the implementation. Override the synchronous
    /// method <see cref="Execute"/> only, if the 
    /// command also has an equal synchronous implementation.
    /// </remarks>
    public virtual Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using (CreateExecuteScope())
        {
            if (cancellationToken.IsCancellationRequested)
                return TaskPolyfill.FromCanceled(cancellationToken);

            try
            {
                Execute();

                return TaskPolyfill.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskPolyfill.FromException(ex);
            }
        }
    }
}