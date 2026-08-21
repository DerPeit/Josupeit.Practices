// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs.TaskPool.Tests;

/// <summary>
/// Produces canceled tasks from a job's own cancellation token.
/// </summary>
internal static class CanceledTask
{
    private static readonly TimeSpan CancellationTimeout = TimeSpan.FromSeconds(5);

    public static Task From(CancellationToken cancellationToken)
    {
        WaitForCancellation(cancellationToken);

        return Task.FromCanceled(cancellationToken);
    }

    public static Task<TResult> From<TResult>(CancellationToken cancellationToken)
    {
        WaitForCancellation(cancellationToken);

        return Task.FromCanceled<TResult>(cancellationToken);
    }

    /// <remarks>
    /// <see cref="JobBase.Cancel" /> queues the cancellation to the thread pool and returns, so a
    /// caller cannot assume the token is already canceled. <see cref="Task.FromCanceled" /> throws
    /// unless it is, which made the cancellation tests race the thread pool and fail under load on
    /// CI. Waiting keeps the assertions honest - a job that never cancels still fails the test on
    /// the timeout instead of hanging.
    /// </remarks>
    private static void WaitForCancellation(CancellationToken cancellationToken)
    {
        cancellationToken.WaitHandle.WaitOne(CancellationTimeout);
    }
}
