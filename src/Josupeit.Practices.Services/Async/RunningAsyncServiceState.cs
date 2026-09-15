// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal sealed class RunningAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;

    public RunningAsyncServiceState(IAsyncServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Running;

    public async Task PauseAsync(CancellationToken cancellationToken)
    {
        using var pauseAttempt = cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new();

        _owner.TransitionTo(new PausingAsyncServiceState(_owner, pauseAttempt));

        try
        {
            await _owner.OnPauseAsync(pauseAttempt.Token).ConfigureAwait(false);

            _owner.TransitionTo(new PausedAsyncServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new RunningAsyncServiceState(_owner));
            throw;
        }
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently running and thus, cannot be resumed. " +
            $"In this state it can only be paused or stopped. Please pause it first and resume it then.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stopAttempt = cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new();

        _owner.TransitionTo(new StoppingFromRunningAsyncServiceState(_owner, stopAttempt));

        try
        {
            await _owner.OnStopAsync(stopAttempt.Token).ConfigureAwait(false);

            _owner.TransitionTo(new StoppedAsyncServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new RunningAsyncServiceState(_owner));
            throw;
        }
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
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently running.");
    }

    public void ThrowIfStopping()
    {
    }

    public void ThrowIfStopped()
    {
    }
}
