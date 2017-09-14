using System;

namespace Josupeit.Practices.Services;

internal class StoppedServiceState : IServiceState
{
    private readonly IServiceInternals _owner;

    public StoppedServiceState(IServiceInternals owner)
    {
        _owner = owner;
    }

    public ServiceState State => ServiceState.Stopped;

    public void Pause()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently stopped and thus, cannot be paused. " +
            $"In this state it can only be started. Please start it first and pause it then.");
    }

    public void Resume()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently stopped and thus, cannot be resumed. " +
            $"In this state it can only be started. Please start and pause it first and resume it then.");
    }

    public void Start()
    {
        _owner.TransitionTo(new StartingServiceState(_owner));

        try
        {
            _owner.OnStart();
            _owner.TransitionTo(new RunningServiceState(_owner));
        }
        catch
        {
            _owner.TransitionTo(new StoppedServiceState(_owner));
            throw;
        }
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
    }

    public void ThrowIfStopped()
    {
        throw new InvalidOperationException(
            $"This instance of {_owner.GetType()} is currently stopped.");
    }
}