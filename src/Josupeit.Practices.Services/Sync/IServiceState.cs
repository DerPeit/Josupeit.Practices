namespace Josupeit.Practices.Services;

internal interface IServiceState
{
    ServiceState State { get; }

    void Pause();

    void Resume();

    void Start();

    void Stop();

    void ThrowIfPausing();

    void ThrowIfPaused();

    void ThrowIfResuming();

    void ThrowIfStarting();

    void ThrowIfRunning();

    void ThrowIfStopping();

    void ThrowIfStopped();
}