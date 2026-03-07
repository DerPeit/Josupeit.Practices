// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal class StoppedAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;

    public StoppedAsyncServiceState(IAsyncServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Stopped;

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently stopped and thus, cannot be paused. " +
            $"In this state it can only be started. Please start it first and pause it then.");
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently stopped and thus, cannot be resumed. " +
            $"In this state it can only be started. Please start and pause it first and resume it then.");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var startAttempt = cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new();

        _owner.TransitionTo(new StartingAsyncServiceState(_owner, startAttempt));

        try
        {
            await _owner.OnStartAsync(startAttempt.Token).ConfigureAwait(false);

            _owner.TransitionTo(new RunningAsyncServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new StoppedAsyncServiceState(_owner));
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
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
    }

    public void ThrowIfRunning()
    {
    }

    public void ThrowIfStopping()
    {
    }

    public void ThrowIfStopped()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently stopped.");
    }
}
