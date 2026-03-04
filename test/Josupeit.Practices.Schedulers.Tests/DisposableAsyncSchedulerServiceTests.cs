using Josupeit.Practices.Services;
using NSubstitute;
using NUnit.Framework;

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