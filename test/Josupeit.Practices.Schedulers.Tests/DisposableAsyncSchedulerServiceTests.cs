// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Josupeit.Practices.Services;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(DisposableAsyncSchedulerService))]
public class DisposableAsyncSchedulerServiceTests : SchedulerTests<DisposableAsyncSchedulerService>
{
    [Test]
    public void Scheduler_inherits_from_DisposableAsyncService()
    {
        var subject = Substitute.ForPartsOf<DisposableAsyncSchedulerService>();

        Assert.That(subject, Is.AssignableTo<DisposableAsyncService>());
    }
}
