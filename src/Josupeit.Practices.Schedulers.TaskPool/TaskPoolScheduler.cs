// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// This class represents a simple scheduler that immediately dispatches a scheduled item to the thread pool via <see cref="Task.Run(Func{Task})"/>.<para />
/// Implements the <see cref="Josupeit.Practices.Schedulers.Scheduler" />.
/// </summary>
/// <seealso cref="Josupeit.Practices.Schedulers.Scheduler" />
[PublicAPI]
public class TaskPoolScheduler : Scheduler
{
    /// <inheritdoc />
    protected override void Enqueue(IScheduled scheduled)
    {
        ArgumentNullException.ThrowIfNull(scheduled);

        _ = Task.Run(() => scheduled.ExecuteAsync(CancellationToken.None));
    }
}
