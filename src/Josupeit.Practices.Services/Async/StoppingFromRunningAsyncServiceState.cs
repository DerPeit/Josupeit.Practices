using System;
using System.Threading;
using System.Threading.Tasks;
using Josupeit.Practices.Polyfills.Tasks;

namespace Josupeit.Practices.Services;

internal class StoppingFromRunningAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;
    private readonly CancellationTokenSource _stopAttempt;

    public StoppingFromRunningAsyncServiceState(IAsyncServiceInternals owner, CancellationTokenSource stopAttempt)
    {
        _owner = owner;
        _stopAttempt = stopAttempt;
    }

    public ServiceState State => ServiceState.Stopping;

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped and thus, cannot be paused. " +
            $"Please wait for the progress to finish, start it first and pause it then.");
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped and thus, cannot be resumed. " +
            $"Please wait for the progress to finish, start and pause it first and resume it then.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stopAttempt.Cancel();

        return TaskPolyfill.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return TaskPolyfill.CompletedTask;
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
    }

    public void ThrowIfRunning()
    {
    }

    public void ThrowIfStopping()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped.");
    }

    public void ThrowIfStopped()
    {
    }
}