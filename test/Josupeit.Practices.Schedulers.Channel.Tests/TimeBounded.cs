// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers.Channel.Tests;

/// <summary>
/// Bounds the waits these tests perform on background work.
/// </summary>
/// <remarks>
/// Neither NUnit's [Timeout] nor its [CancelAfter] can unstick an await that never completes:
/// Thread.Abort is gone, so [CancelAfter] only cancels a token the test has to observe itself, and
/// it does not exist in the NUnit 3 that netcoreapp2.2 is pinned to. Without a bound, a regression
/// that stops work from running hangs the whole job instead of failing one test.
/// </remarks>
internal static class TimeBounded
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

    public static async Task WithinTimeout(this Task task)
    {
        var timeout = Task.Delay(_timeout);

        var completed = await Task.WhenAny(task, timeout).ConfigureAwait(false);

        if (completed == timeout)
            Assert.Fail($"Timed out after {_timeout.TotalSeconds:0} seconds waiting for background work.");

        await task.ConfigureAwait(false);
    }
}
