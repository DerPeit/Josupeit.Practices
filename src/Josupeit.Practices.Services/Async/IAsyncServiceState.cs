// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Services;

internal interface IAsyncServiceState
{
    ServiceState State { get; }

    Task PauseAsync(CancellationToken cancellationToken);

    Task ResumeAsync(CancellationToken cancellationToken);

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    void ThrowIfPausing();

    void ThrowIfPaused();

    void ThrowIfResuming();

    void ThrowIfStarting();

    void ThrowIfRunning();

    void ThrowIfStopping();

    void ThrowIfStopped();
}
