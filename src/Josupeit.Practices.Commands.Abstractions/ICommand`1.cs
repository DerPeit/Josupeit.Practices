using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This interface represents a command without return value.
/// </summary>
/// <typeparam name="TInput">The type of the input parameter.</typeparam>
[PublicAPI]
public interface ICommand<in TInput>
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    void Execute(TInput input);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    Task ExecuteAsync(TInput input);

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="input">An input parameter.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    Task ExecuteAsync(TInput input, CancellationToken cancellationToken);
}