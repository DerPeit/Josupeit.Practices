// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Services;

internal class StartingServiceState : IServiceState
{
    private readonly IServiceInternals _owner;

    public StartingServiceState(IServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Starting;

    public void Pause()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently starting and thus, cannot be paused. " +
            $"Please wait until started and pause then.");
    }

    public void Resume()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently starting and thus, cannot be resumed. " +
            $"Please wait until started, pause it first and resume it then.");
    }

    public void Start()
    {
    }

    public void Stop()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently starting and thus, cannot be stopped. " +
            $"Please wait until started, stop it then.");
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
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently being started.");
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
