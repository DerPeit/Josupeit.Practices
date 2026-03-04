#nullable enable

namespace Namespace;

partial class Class : global::System.IDisposable, global::System.IAsyncDisposable
{
    private volatile bool _isDisposed;
	
    /// <summary>
    /// Finalizes this instance and ensures unmanaged resources are released if not already done.
    /// </summary>
    ~Class()
    {
        this.Dispose(isDisposing: false);
    }

    /// <summary>
    /// Occurs when this instance is about to release its resources.
    /// </summary>
    public event global::System.EventHandler? Disposing;

    /// <summary>
    /// Gets a value indicating whether this instance has released its resources.
    /// </summary>
    /// <value><c>true</c> if this instance has released its resources; otherwise, <c>false</c>.</value>
    public bool IsDisposed => this._isDisposed;

    /// <inheritdoc cref="global::System.IDisposable.Dispose"/>
    public void Dispose()
    {
        this.Dispose(isDisposing: true);
        global::System.GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="global::System.IAsyncDisposable.DisposeAsync"/>
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore().ConfigureAwait(false);
        global::System.GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined task associated with freeing, releasing, or resetting managed resources.
    /// </summary>
    // Stub
    protected virtual void DisposeManagedResources()
    {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources asynchronously.
    /// </summary>
    /// <returns>
    /// A <see cref="global::System.Threading.Tasks.ValueTask">ValueTask</see> that completes when all asynchronous cleanup operations have finished.
    /// </returns>
    // Stub
    protected virtual global::System.Threading.Tasks.ValueTask DisposeManagedResourcesAsync()
    {
        return new global::System.Threading.Tasks.ValueTask();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    // Stub
    protected virtual void DisposeUnmanagedResources()
    {
    }

    /// <summary>
    /// Throws an <see cref="global::System.ObjectDisposedException">ObjectDisposedException</see> if this instance has already released its resources.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (this._isDisposed)
            throw new global::System.ObjectDisposedException(this.GetType().ToString());
    }

    private void Dispose(bool isDisposing)
    {
        // MARKER: Idempotence
        {
            var isAlreadyDisposed = this._isDisposed;
            this._isDisposed = true;
            if (isAlreadyDisposed)
                return;
        }

        if (!isDisposing)
        {
            this.DisposeUnmanagedResources();
            return;
        }

        try
        {
            this.OnDisposing();
        }
        finally
        {
            this.DisposeManagedResources();
            this.DisposeUnmanagedResources();
        }
    }

    private async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()
    {
        // MARKER: Idempotence
        {
            var isAlreadyDisposed = this._isDisposed;
            this._isDisposed = true;
            if (isAlreadyDisposed)
                return;
        }

        try
        {
            this.OnDisposing();
        }
        finally
        {
            await this.DisposeManagedResourcesAsync().ConfigureAwait(false);
            this.DisposeUnmanagedResources();
        }
    }

    private void OnDisposing()
    {
        this.Disposing?.Invoke(this, global::System.EventArgs.Empty);
    }
}
