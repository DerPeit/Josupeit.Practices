// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs;

#pragma warning disable CA1068 // CancellationToken groups logically with the promise it guards, not the outcome callbacks.

internal static class ScheduleErrorHandler
{
    public static async void HandleScheduleErrors(
        Task schedulePromise,
        CancellationToken jobCancellationToken,
        Action notifyCanceled,
        Action<Exception> notifyFailed)
    {
        try
        {
            await schedulePromise.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (jobCancellationToken.IsCancellationRequested)
        {
            notifyCanceled();
        }
        catch (Exception ex)
        {
            notifyFailed(
                new InvalidOperationException(
                    "The attempt to schedule the current job on the scheduler failed unexpectedly. " +
                    "The scheduler may not have been running or may have declined scheduling due to backpressure. " +
                    "See the inner exception for details.",
                    ex));
        }
    }
}
