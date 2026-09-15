// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

#pragma warning disable CA1849 // Cancel() is deliberate; CancelAsync() does not exist on netstandard2.0.

internal sealed class StoppingFromPauseAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;
    private readonly CancellationTokenSource _stopAttempt;

    public StoppingFromPauseAsyncServiceState(IAsyncServiceInternals owner, CancellationTokenSource stopAttempt)
    {
        _owner = owner;
        _stopAttempt = stopAttempt;
    }

    public ServiceState State => ServiceState.Stopping;

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        _stopAttempt.Cancel();

        return Task.CompletedTask;
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped from pause and thus, cannot be resumed. " +
            $"Please wait for the progress to finish, start it, pause it and resume it then.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped from pause and thus, cannot be started. " +
            $"Please wait for the progress to finish and start it then.");
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
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped.");
    }

    public void ThrowIfStopped()
    {
    }
}
