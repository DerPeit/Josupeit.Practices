#if NET8_0_OR_GREATER

using Josupeit.Practices.Disposables;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncDisposableAsyncScheduler))]
public class AsyncDisposableAsyncSchedulerTests : SchedulerTests<AsyncDisposableAsyncScheduler>
{
    [Test]
    public void Scheduler_inherits_from_ImplicitAsyncDisposable()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableScheduler>();

        Assert.That(subject, Is.AssignableTo<ImplicitAsyncDisposable>());
    }
}

#endif