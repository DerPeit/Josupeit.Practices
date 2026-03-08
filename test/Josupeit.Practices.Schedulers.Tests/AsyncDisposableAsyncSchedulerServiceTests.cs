// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

#if NET8_0_OR_GREATER

using Josupeit.Practices.Services;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncDisposableAsyncSchedulerService))]
public class AsyncDisposableAsyncSchedulerServiceTests : SchedulerTests<AsyncDisposableAsyncSchedulerService>
{
    [Test]
    public void Scheduler_inherits_from_AsyncDisposableAsyncService()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableAsyncSchedulerService>();

        Assert.That(subject, Is.AssignableTo<AsyncDisposableAsyncService>());
    }
}

#endif
