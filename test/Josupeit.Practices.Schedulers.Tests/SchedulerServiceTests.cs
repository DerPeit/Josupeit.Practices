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