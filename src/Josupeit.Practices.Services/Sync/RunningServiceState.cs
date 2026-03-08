// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Services;

internal class RunningServiceState : IServiceState
{
    private readonly IServiceInternals _owner;

    public RunningServiceState(IServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Running;

    public void Pause()
    {
        _owner.TransitionTo(new PausingServiceState(_owner));

        try
        {
            _owner.OnPause();
            _owner.TransitionTo(new PausedServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new RunningServiceState(_owner));
            throw;
        }
    }

    public void Resume()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently running and thus, cannot be resumed. " +
            $"In this state it can only be paused or stopped. Please pause it first and resume it then.");
    }

    public void Start()
    {
    }

    public void Stop()
    {
        _owner.TransitionTo(new StoppingServiceState(_owner));

        try
        {
            _owner.OnStop();
            _owner.TransitionTo(new StoppedServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new RunningServiceState(_owner));
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
