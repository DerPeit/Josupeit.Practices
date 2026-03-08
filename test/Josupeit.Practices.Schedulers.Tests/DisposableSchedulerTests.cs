// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(DisposableScheduler))]
public class DisposableSchedulerTests : SchedulerTests<DisposableScheduler>
{
    [Test]
    public void Scheduler_implements_IDisposable()
    {
        var subject = Substitute.ForPartsOf<DisposableScheduler>();

        Assert.That(subject, Is.AssignableTo<IDisposable>());
    }
}
