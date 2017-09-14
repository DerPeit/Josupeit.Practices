using System;
using System.Threading;
using System.Threading.Tasks;
using Josupeit.Practices.Disposables;
using Josupeit.Practices.Polyfills.Tasks;

namespace Josupeit.Practices.Services;

/// <summary>
/// This class is a base class and represents an async service.
/// </summary>
/// <seealso cref="IAsyncService" />
public abstract partial class AsyncService : IAsyncService
{
    private volatile IAsyncServiceState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="Service"/> class.
    /// </summary>
    protected AsyncService()
    {
        _state = new StoppedAsyncServiceState(this);
    }

    /// <summary>
    /// Gets a value indicating whether this instance can be paused.
    /// </summary>
    /// <value><c>true</c> if this instance can be paused; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Derived classes can override this property to indicate that this instance can
    /// be paused. If so, this base class will take care of calling the <see cref="OnPauseAsync(CancellationToken)"/>
    /// and <see cref="OnResumeAsync(CancellationToken)"/> methods respectively.
    /// Otherwise, calls to <see cref="PauseAsync(CancellationToken)"/> and <see cref="ResumeAsync(CancellationToken)"/>
    /// will throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    public virtual bool CanPause => false;

    /// <inheritdoc />
    public ServiceState State => _state.State;

    /// <inheritdoc />
    public Task PauseAsync() => PauseAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task PauseAsync(CancellationToken cancellationToken)
    {
        if (!CanPause)
            throw new NotSupportedException(
                $"This instance of {GetType()} cannot pause.");

        return _state.PauseAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task ResumeAsync() => ResumeAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        if (!CanPause)
            throw new NotSupportedException(
                $"This instance of {GetType()} cannot pause and thus can also not resume.");

        return _state.ResumeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StartAsync() => StartAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) => _state.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync() => StopAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => _state.StopAsync(cancellationToken);

    /// <summary>
    /// Called when this instance is being paused.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal virtual Task OnPauseAsync(CancellationToken cancellationToken)
    {
        return TaskPolyfill.CompletedTask;
    }

    /// <summary>
    /// Called when this instance is being resumed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal virtual Task OnResumeAsync(CancellationToken cancellationToken)
    {
        return TaskPolyfill.CompletedTask;
    }

    /// <summary>
    /// Called when this instance is being started.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task OnStartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called when this instance is being stopped.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task OnStopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being paused or paused.
    /// </summary>
    protected void ThrowIfPaused()
    {
        var state = _state;

        state.ThrowIfPausing();
        state.ThrowIfPaused();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being started, being resumed or running.
    /// </summary>
    protected void ThrowIfRunning()
    {
        var state = _state;

        state.ThrowIfStarting();
        state.ThrowIfResuming();
        state.ThrowIfRunning();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is starting, being paused, paused,
    /// being resumed, being stopped or stopped.
    /// </summary>
    protected void ThrowIfNotRunning()
    {
        var state = _state;

        state.ThrowIfStarting();
        state.ThrowIfPausing();
        state.ThrowIfPaused();
        state.ThrowIfResuming();
        state.ThrowIfStopping();
        state.ThrowIfStopped();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being stopped or stopped.
    /// </summary>
    protected void ThrowIfStopped()
    {
        var state = _state;

        state.ThrowIfStopping();
        state.ThrowIfStopped();
    }
}

public abstract partial class AsyncService : IAsyncServiceInternals
{
    Task IAsyncServiceInternals.OnPauseAsync(CancellationToken cancellationToken) => OnPauseAsync(cancellationToken);

    Task IAsyncServiceInternals.OnResumeAsync(CancellationToken cancellationToken) => OnResumeAsync(cancellationToken);

    Task IAsyncServiceInternals.OnStartAsync(CancellationToken cancellationToken) => OnStartAsync(cancellationToken);

    Task IAsyncServiceInternals.OnStopAsync(CancellationToken cancellationToken) => OnStopAsync(cancellationToken);

    void IAsyncServiceInternals.TransitionTo(IAsyncServiceState nextState) => _state = nextState;
}

/// <summary>
/// This class is a base class and represents an async service which owns resources that needs to be released.
/// </summary>
/// <seealso cref="IAsyncService" />
public abstract partial class DisposableAsyncService : ImplicitDisposable, IAsyncService
{
    private volatile IAsyncServiceState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="Service"/> class.
    /// </summary>
    protected DisposableAsyncService()
    {
        _state = new StoppedAsyncServiceState(this);
    }

    /// <summary>
    /// Gets a value indicating whether this instance can be paused.
    /// </summary>
    /// <value><c>true</c> if this instance can be paused; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Derived classes can override this property to indicate that this instance can
    /// be paused. If so, this base class will take care of calling the <see cref="OnPauseAsync(CancellationToken)"/>
    /// and <see cref="OnResumeAsync(CancellationToken)"/> methods respectively.
    /// Otherwise, calls to <see cref="PauseAsync(CancellationToken)"/> and <see cref="ResumeAsync(CancellationToken)"/>
    /// will throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    public virtual bool CanPause => false;

    /// <inheritdoc />
    public ServiceState State => _state.State;

    /// <inheritdoc />
    public Task PauseAsync() => PauseAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task PauseAsync(CancellationToken cancellationToken)
    {
        if (!CanPause)
            throw new NotSupportedException(
                $"This instance of {GetType()} cannot pause.");

        return _state.PauseAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task ResumeAsync() => ResumeAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        if (!CanPause)
            throw new NotSupportedException(
                $"This instance of {GetType()} cannot pause and thus can also not resume.");

        return _state.ResumeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StartAsync() => StartAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) => _state.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync() => StopAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => _state.StopAsync(cancellationToken);

    /// <summary>
    /// Called when this instance is being paused.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal virtual Task OnPauseAsync(CancellationToken cancellationToken)
    {
        return TaskPolyfill.CompletedTask;
    }

    /// <summary>
    /// Called when this instance is being resumed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal virtual Task OnResumeAsync(CancellationToken cancellationToken)
    {
        return TaskPolyfill.CompletedTask;
    }

    /// <summary>
    /// Called when this instance is being started.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task OnStartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called when this instance is being stopped.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task OnStopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being paused or paused.
    /// </summary>
    protected void ThrowIfPaused()
    {
        var state = _state;

        state.ThrowIfPausing();
        state.ThrowIfPaused();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being started, being resumed or running.
    /// </summary>
    protected void ThrowIfRunning()
    {
        var state = _state;

        state.ThrowIfStarting();
        state.ThrowIfResuming();
        state.ThrowIfRunning();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is starting, being paused, paused,
    /// being resumed, being stopped or stopped.
    /// </summary>
    protected void ThrowIfNotRunning()
    {
        var state = _state;

        state.ThrowIfStarting();
        state.ThrowIfPausing();
        state.ThrowIfPaused();
        state.ThrowIfResuming();
        state.ThrowIfStopping();
        state.ThrowIfStopped();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being stopped or stopped.
    /// </summary>
    protected void ThrowIfStopped()
    {
        var state = _state;

        state.ThrowIfStopping();
        state.ThrowIfStopped();
    }
}

public abstract partial class DisposableAsyncService : IAsyncServiceInternals
{
    Task IAsyncServiceInternals.OnPauseAsync(CancellationToken cancellationToken) => OnPauseAsync(cancellationToken);

    Task IAsyncServiceInternals.OnResumeAsync(CancellationToken cancellationToken) => OnResumeAsync(cancellationToken);

    Task IAsyncServiceInternals.OnStartAsync(CancellationToken cancellationToken) => OnStartAsync(cancellationToken);

    Task IAsyncServiceInternals.OnStopAsync(CancellationToken cancellationToken) => OnStopAsync(cancellationToken);

    void IAsyncServiceInternals.TransitionTo(IAsyncServiceState nextState) => _state = nextState;
}

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

/// <summary>
/// This class is a base class and represents an async service which owns resources that needs to be released asynchronously.
/// </summary>
/// <seealso cref="IAsyncService" />
public abstract partial class AsyncDisposableAsyncService : ImplicitAsyncDisposable, IAsyncService
{
    private volatile IAsyncServiceState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="Service"/> class.
    /// </summary>
    protected AsyncDisposableAsyncService()
    {
        _state = new StoppedAsyncServiceState(this);
    }

    /// <summary>
    /// Gets a value indicating whether this instance can be paused.
    /// </summary>
    /// <value><c>true</c> if this instance can be paused; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Derived classes can override this property to indicate that this instance can
    /// be paused. If so, this base class will take care of calling the <see cref="OnPauseAsync(CancellationToken)"/>
    /// and <see cref="OnResumeAsync(CancellationToken)"/> methods respectively.
    /// Otherwise, calls to <see cref="PauseAsync(CancellationToken)"/> and <see cref="ResumeAsync(CancellationToken)"/>
    /// will throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    public virtual bool CanPause => false;

    /// <inheritdoc />
    public ServiceState State => _state.State;

    /// <inheritdoc />
    public Task PauseAsync() => PauseAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task PauseAsync(CancellationToken cancellationToken)
    {
        if (!CanPause)
            throw new NotSupportedException(
                $"This instance of {GetType()} cannot pause.");

        return _state.PauseAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task ResumeAsync() => ResumeAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        if (!CanPause)
            throw new NotSupportedException(
                $"This instance of {GetType()} cannot pause and thus can also not resume.");

        return _state.ResumeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StartAsync() => StartAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) => _state.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync() => StopAsync(CancellationToken.None);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => _state.StopAsync(cancellationToken);

    /// <summary>
    /// Called when this instance is being paused.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal virtual Task OnPauseAsync(CancellationToken cancellationToken)
    {
        return TaskPolyfill.CompletedTask;
    }

    /// <summary>
    /// Called when this instance is being resumed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal virtual Task OnResumeAsync(CancellationToken cancellationToken)
    {
        return TaskPolyfill.CompletedTask;
    }

    /// <summary>
    /// Called when this instance is being started.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task OnStartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called when this instance is being stopped.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task OnStopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being paused or paused.
    /// </summary>
    protected void ThrowIfPaused()
    {
        var state = _state;

        state.ThrowIfPausing();
        state.ThrowIfPaused();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being started, being resumed or running.
    /// </summary>
    protected void ThrowIfRunning()
    {
        var state = _state;

        state.ThrowIfStarting();
        state.ThrowIfResuming();
        state.ThrowIfRunning();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is starting, being paused, paused,
    /// being resumed, being stopped or stopped.
    /// </summary>
    protected void ThrowIfNotRunning()
    {
        var state = _state;

        state.ThrowIfStarting();
        state.ThrowIfPausing();
        state.ThrowIfPaused();
        state.ThrowIfResuming();
        state.ThrowIfStopping();
        state.ThrowIfStopped();
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this instance is being stopped or stopped.
    /// </summary>
    protected void ThrowIfStopped()
    {
        var state = _state;

        state.ThrowIfStopping();
        state.ThrowIfStopped();
    }
}

public abstract partial class AsyncDisposableAsyncService : IAsyncServiceInternals
{
    Task IAsyncServiceInternals.OnPauseAsync(CancellationToken cancellationToken) => OnPauseAsync(cancellationToken);

    Task IAsyncServiceInternals.OnResumeAsync(CancellationToken cancellationToken) => OnResumeAsync(cancellationToken);

    Task IAsyncServiceInternals.OnStartAsync(CancellationToken cancellationToken) => OnStartAsync(cancellationToken);

    Task IAsyncServiceInternals.OnStopAsync(CancellationToken cancellationToken) => OnStopAsync(cancellationToken);

    void IAsyncServiceInternals.TransitionTo(IAsyncServiceState nextState) => _state = nextState;
}

#endif
