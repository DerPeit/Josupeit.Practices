using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This interface represents a parameterless command without return value.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes this instance synchronously.
    /// </summary>
    void Execute();

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    Task ExecuteAsync();

    /// <summary>
    /// Executes this instance asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous progress.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}