namespace Josupeit.Practices.Services;

internal interface IServiceInternals
{
    void OnPause();

    void OnResume();

    void OnStart();

    void OnStop();

    void TransitionTo(IServiceState nextState);
}