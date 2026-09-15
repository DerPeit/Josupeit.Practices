// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Services;

internal sealed class PausingServiceState : IServiceState
{
    private readonly IServiceInternals _owner;

    public PausingServiceState(IServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Pausing;

    public void Pause()
    {
    }

    public void Resume()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being paused and thus, cannot yet be resumed. " +
            $"Please wait for the progress to finish and resume it then.");
    }

    public void Start()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being paused and thus, cannot be started. " +
            $"Please wait for the progress to finish, stop it first and start it then.");
    }

    public void Stop()
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
