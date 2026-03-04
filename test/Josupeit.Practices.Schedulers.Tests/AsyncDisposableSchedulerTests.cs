#if NET8_0_OR_GREATER

using System;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncDisposableScheduler))]
public class AsyncDisposableSchedulerTests : SchedulerTests<AsyncDisposableScheduler>
{
    [Test]
    public void Scheduler_implements_IAsyncDisposable()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableScheduler>();

        Assert.That(subject, Is.AssignableTo<IAsyncDisposable>());
    }
}

#endif
