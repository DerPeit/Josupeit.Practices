// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

#if NET8_0_OR_GREATER

using Josupeit.Practices.Services;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncDisposableSchedulerService))]
public class AsyncDisposableSchedulerServiceTests : SchedulerTests<AsyncDisposableSchedulerService>
{
    [Test]
    public void Scheduler_inherits_from_AsyncDisposableService()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableSchedulerService>();

        Assert.That(subject, Is.AssignableTo<AsyncDisposableService>());
    }
}

#endif
