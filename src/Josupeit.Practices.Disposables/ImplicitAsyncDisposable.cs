using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Disposables;

/// <summary>
/// This class is a base class to be implemented to release resources with implicitly
/// exposing the <see cref="Dispose()"/>  method and members of the <see cref="INotifyingAsyncDisposable"/>
/// interface.
/// </summary>
/// <seealso cref="ExplicitAsyncDisposable" />
[PublicAPI]
public abstract class ImplicitAsyncDisposable : ExplicitAsyncDisposable
{
    /// <summary>
    /// Occurs when this instance is about to release its resources.
    /// </summary>
    public new event EventHandler? Disposing
    {
        add => base.Disposing += value;
        remove => base.Disposing -= value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance has released its resources.
    /// </summary>
    /// <value><c>true</c> if this instance has released its resources; otherwise, <c>false</c>.</value>
    public new bool IsDisposed => base.IsDisposed;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
    /// </summary>
    public new void Dispose() => base.Dispose();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources asynchronously.
    /// </summary>
    public new ValueTask DisposeAsync() => base.DisposeAsync();
}