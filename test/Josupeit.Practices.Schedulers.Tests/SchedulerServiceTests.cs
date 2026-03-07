// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Josupeit.Practices.Services;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(SchedulerService))]
public class SchedulerServiceTests : SchedulerTests<SchedulerService>
{
    [Test]
    public void Scheduler_inherits_from_Service()
    {
        var subject = Substitute.ForPartsOf<SchedulerService>();

        Assert.That(subject, Is.AssignableTo<Service>());
    }
}
