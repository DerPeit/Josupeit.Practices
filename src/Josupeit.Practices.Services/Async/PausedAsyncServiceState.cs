using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal class PausedAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;

    public PausedAsyncServiceState(IAsyncServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Paused;

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task ResumeAsync(CancellationToken cancellationToken)
    {
        using var resumeAttempt = cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new();

        _owner.TransitionTo(new ResumingAsyncServiceState(_owner, resumeAttempt));

        try
        {
            await _owner.OnResumeAsync(resumeAttempt.Token).ConfigureAwait(false);

            _owner.TransitionTo(new RunningAsyncServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new PausedAsyncServiceState(_owner));
            throw;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently paused and thus, cannot be started. " +
            $"In this state it can only be resumed or stopped. Please stop it first and start it then.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stopAttempt = cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new();

        _owner.TransitionTo(new StoppingFromPauseAsyncServiceState(_owner, stopAttempt));

        try
        {
            await _owner.OnStopAsync(stopAttempt.Token).ConfigureAwait(false);

            _owner.TransitionTo(new StoppedAsyncServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new PausedAsyncServiceState(_owner));
            throw;
        }
    }

    public void ThrowIfPausing()
    {
    }

    public void ThrowIfPaused()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently paused.");
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
    }

    public void ThrowIfStopped()
    {
    }
}