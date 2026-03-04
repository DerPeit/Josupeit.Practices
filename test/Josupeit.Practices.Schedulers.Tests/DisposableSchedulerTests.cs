using Josupeit.Practices.Disposables;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(DisposableScheduler))]
public class DisposableSchedulerTests : SchedulerTests<DisposableScheduler>
{
    [Test]
    public void Scheduler_inherits_from_ImplicitDisposable()
    {
        var subject = Substitute.ForPartsOf<DisposableScheduler>();

        Assert.That(subject, Is.AssignableTo<ImplicitDisposable>());
    }
}