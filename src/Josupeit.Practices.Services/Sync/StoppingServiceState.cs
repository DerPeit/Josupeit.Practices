// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Services;

internal class StoppingServiceState : IServiceState
{
    private readonly IServiceInternals _owner;

    public StoppingServiceState(IServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Stopping;

    public void Pause()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped and thus, cannot be paused. " +
            $"Please wait for the progress to finish, start it first and pause it then.");
    }

    public void Resume()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped and thus, cannot be resumed. " +
            $"Please wait for the progress to finish, start and pause it first and resume it then.");
    }

    public void Start()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being stopped and thus, cannot be started. " +
            $"Please wait for the progress to finish and start it then.");
    }

    public void Stop()
    {
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
