// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Services;

internal interface IServiceInternals
{
    void OnPause();

    void OnResume();

    void OnStart();

    void OnStop();

    void TransitionTo(IServiceState nextState);
}
