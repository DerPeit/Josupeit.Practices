// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Services;

internal class ResumingServiceState : IServiceState
{
    private readonly IServiceInternals _owner;

    public ResumingServiceState(IServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Resuming;

    public void Pause()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being resumed and thus, cannot be paused. " +
            $"Please wait for the progress to finish and pause it then.");
    }

    public void Resume()
    {
    }

    public void Start()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being resumed and thus, cannot be started. " +
            $"Please wait for the progress to finish, stop it first and start it then.");
    }

    public void Stop()
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
