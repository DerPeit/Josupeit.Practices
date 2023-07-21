using System;

namespace Josupeit.Practices.Disposables;

/// <summary>
/// This class is a base class to be implemented to release resources without exposing
/// the <see cref="Dispose()" /> method  or members of the <see cref="INotifyingDisposable" />
/// interface.
/// </summary>
/// <seealso cref="INotifyingDisposable" />
/// <remarks>This class should be inherited when the derived class provides a semantically equal
/// method to release resources, like Close or Stop and delegate these exposed implementations
/// to the protected members <see cref="Dispose()" />, <see cref="IsDisposed" /> and
/// <see cref="Disposing" /> respectively.</remarks>
public abstract class ExplicitDisposable : INotifyingDisposable
{
    private volatile bool _isDisposed;

    /// <summary>
    /// Finalizes an instance of the <see cref="ExplicitDisposable"/> class.
    /// </summary>
    ~ExplicitDisposable()
    {
        Dispose(isDisposing: false);
    }

    event EventHandler? INotifyingDisposable.Disposing
    {
        add => Disposing += value;
        remove => Disposing -= value;
    }

    bool INotifyingDisposable.IsDisposed => _isDisposed;

    void IDisposable.Dispose() => Dispose();

    /// <summary>
    /// Occurs when this instance is about to release its resources.
    /// </summary>
    protected event EventHandler? Disposing;

    /// <summary>
    /// Gets a value indicating whether this instance has released its resources.
    /// </summary>
    /// <value><c>true</c> if this instance has released its resources; otherwise, <c>false</c>.</value>
    protected bool IsDisposed => _isDisposed;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
    /// </summary>
    protected void Dispose()
    {
        Dispose(isDisposing: true);
        GCProvider.Current.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined task associated with freeing, releasing, or resetting managed resources.
    /// </summary>
    protected virtual void DisposeManagedResources()
    {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    protected virtual void DisposeUnmanagedResources()
    {
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this instance has released its resources.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().ToString());
    }

    private void Dispose(bool isDisposing)
    {
        var isDisposed = _isDisposed;
        _isDisposed = true;

        if (isDisposed)
            return;

        if (!isDisposing)
        {
            DisposeUnmanagedResources();
            return;
        }

        try
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            DisposeManagedResources();
            DisposeUnmanagedResources();
        }
    }
}