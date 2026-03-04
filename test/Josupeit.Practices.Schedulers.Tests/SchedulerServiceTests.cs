using Josupeit.Practices.Services;
using NSubstitute;
using NUnit.Framework;

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