using Josupeit.Practices.Services;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncSchedulerService))]
public class AsyncSchedulerServiceTests : SchedulerTests<AsyncSchedulerService>
{
    [Test]
    public void Scheduler_inherits_from_AsyncService()
    {
        var subject = Substitute.ForPartsOf<AsyncSchedulerService>();

        Assert.That(subject, Is.AssignableTo<AsyncService>());
    }
}