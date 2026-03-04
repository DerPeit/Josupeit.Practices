using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Disposables;

/// <summary>
/// This class is a base class to be implemented to release resources without exposing
/// the Dispose() or <see cref="DisposeAsync()"/> method  or members of the
/// <see cref="INotifyingAsyncDisposable" /> interface.
/// </summary>
/// <seealso cref="INotifyingAsyncDisposable" />
[PublicAPI]
public abstract class ExplicitAsyncDisposable : INotifyingAsyncDisposable
{
    private volatile bool _isDisposed;

    /// <summary>
    /// Finalizes an instance of the <see cref="ExplicitDisposable"/> class.
    /// </summary>
    ~ExplicitAsyncDisposable()
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

    ValueTask IAsyncDisposable.DisposeAsync() => DisposeAsync();

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
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources asynchronously.
    /// </summary>
    protected async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GCProvider.Current.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined task associated with freeing, releasing, or resetting managed resources.
    /// </summary>
    protected virtual void DisposeManagedResources()
    {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources asynchronously.
    /// </summary>
    protected virtual ValueTask DisposeManagedResourcesAsync()
    {
        return default;
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

    private async ValueTask DisposeAsyncCore()
    {
        var isDisposed = _isDisposed;
        _isDisposed = true;

        if (isDisposed)
            return;

        try
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            await DisposeManagedResourcesAsync().ConfigureAwait(false);
            DisposeUnmanagedResources();
        }
    }
}