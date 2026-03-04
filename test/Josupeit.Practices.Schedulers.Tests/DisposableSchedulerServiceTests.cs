using Josupeit.Practices.Services;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(DisposableSchedulerService))]
public class DisposableSchedulerServiceTests : SchedulerTests<DisposableSchedulerService>
{
    [Test]
    public void Scheduler_inherits_from_DisposableService()
    {
        var subject = Substitute.ForPartsOf<DisposableSchedulerService>();

        Assert.That(subject, Is.AssignableTo<DisposableService>());
    }
}