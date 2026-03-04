using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal class StartingAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;
    private readonly CancellationTokenSource _startAttempt;

    public StartingAsyncServiceState(IAsyncServiceInternals owner, CancellationTokenSource startAttempt)
    {
        _owner = owner;
        _startAttempt = startAttempt;
    }

    public ServiceState State => ServiceState.Starting;

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently starting and thus, cannot be paused. " +
            $"Please wait until started and pause then.");
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently starting and thus, cannot be resumed. " +
            $"Please wait until started, pause it first and resume it then.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _startAttempt.Cancel();
        
        return Task.CompletedTask;
    }

    public void ThrowIfPausing()
    {
    }

    public void ThrowIfPaused()
    {
    }

    public void ThrowIfResuming()
    {
    }

    public void ThrowIfStarting()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being started.");
    }

    public void ThrowIfRunning()
    {
    }

    public void ThrowIfStopping()
    {
    }

    public void ThrowIfStopped()
    {
    }
}