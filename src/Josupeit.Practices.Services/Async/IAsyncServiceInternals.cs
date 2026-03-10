// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal interface IAsyncServiceInternals
{
    Task OnPauseAsync(CancellationToken cancellationToken);

    Task OnResumeAsync(CancellationToken cancellationToken);

    Task OnStartAsync(CancellationToken cancellationToken);

    Task OnStopAsync(CancellationToken cancellationToken);

    void TransitionTo(IAsyncServiceState nextState);
}
