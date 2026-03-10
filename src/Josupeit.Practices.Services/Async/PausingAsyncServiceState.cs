// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal class PausingAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;
    private readonly CancellationTokenSource _pauseAttempt;

    public PausingAsyncServiceState(IAsyncServiceInternals owner, CancellationTokenSource pauseAttempt)
    {
        _owner = owner;
        _pauseAttempt = pauseAttempt;
    }

    public ServiceState State => ServiceState.Pausing;

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        _pauseAttempt.Cancel();

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being paused and thus, cannot be started. " +
            $"Please wait for the progress to finish, stop it first and start it then.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being paused and thus, cannot be started. " +
            $"Please wait for the progress to finish and stop it then.");
    }

    public void ThrowIfPausing()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being paused.");
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
    }
}
