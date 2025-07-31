#nullable enable

namespace Namespace;

partial class Class : global::System.IDisposable, global::System.IAsyncDisposable
{
    private volatile bool _isDisposed;
	
    ~Class()
    {
        this.Dispose(isDisposing: false);
        global::System.GC.SuppressFinalize(this);
    }

    public event global::System.EventHandler? Disposing;
	
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
        await this.DisposeAsyncCore();
        global::System.GC.SuppressFinalize(this);
    }

    // Stub
    protected virtual void DisposeManagedResources()
    {
    }

    // Stub
    protected virtual global::System.Threading.Tasks.ValueTask DisposeManagedResourcesAsync()
    {
        return new global::System.Threading.Tasks.ValueTask();
    }

    // Stub
    protected virtual void DisposeUnmanagedResources()
    {
    }

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
            await this.DisposeManagedResourcesAsync();
            this.DisposeUnmanagedResources();
        }
    }

    private void OnDisposing()
    {
        this.Disposing?.Invoke(this, global::System.EventArgs.Empty);
    }
}
