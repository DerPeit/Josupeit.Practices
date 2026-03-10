// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;

namespace Josupeit.Practices.Services;

internal class PausedServiceState : IServiceState
{
    private readonly IServiceInternals _owner;

    public PausedServiceState(IServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Paused;

    public void Pause()
    {
    }

    public void Resume()
    {
        _owner.TransitionTo(new ResumingServiceState(_owner));

        try
        {
            _owner.OnResume();
            _owner.TransitionTo(new RunningServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new PausedServiceState(_owner));
            throw;
        }
    }

    public void Start()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently paused and thus, cannot be started. " +
            $"In this state it can only be resumed or stopped. Please stop it first and start it then.");
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
            _owner.TransitionTo(new PausedServiceState(_owner));
            throw;
        }
    }

    public void ThrowIfPausing()
    {
    }

    public void ThrowIfPaused()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently paused.");
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
