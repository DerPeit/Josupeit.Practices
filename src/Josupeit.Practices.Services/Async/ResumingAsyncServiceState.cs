// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal class ResumingAsyncServiceState : IAsyncServiceState
{
    private readonly IAsyncServiceInternals _owner;
    private readonly CancellationTokenSource _resumeAttempt;

    public ResumingAsyncServiceState(IAsyncServiceInternals owner, CancellationTokenSource resumeAttempt)
    {
        _owner = owner;
        _resumeAttempt = resumeAttempt;
    }

    public ServiceState State => ServiceState.Resuming;

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        _resumeAttempt.Cancel();

        return Task.CompletedTask;
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being resumed and thus, cannot be started. " +
            $"Please wait for the progress to finish, stop it first and start it then.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being resumed and thus, cannot be stopped. " +
            $"Please wait for the progress to finish and stop it then.");
    }

    public void ThrowIfPausing()
    {
    }

    public void ThrowIfPaused()
    {
    }

    public void ThrowIfResuming()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being resumed.");
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
