// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers.Tests;

[TestFixture]
[TestOf(typeof(Scheduler))]
public class SchedulerDefaultConstructorTests
{
    private sealed class TestScheduler : Scheduler
    {
        protected override void Enqueue(IScheduled scheduled)
        {
            scheduled.Execute(CancellationToken.None);
        }
    }

    [Test]
    public void Schedule_invokes_action()
    {
        var subject = new TestScheduler();
        var invoked = false;

        subject.Schedule(() => invoked = true);

        Assert.That(invoked, Is.True);
    }

    [Test]
    public async Task ScheduleAsync_invokes_action()
    {
        var subject = new TestScheduler();
        var invoked = false;

        await subject.ScheduleAsync(() => invoked = true, CancellationToken.None);

        Assert.That(invoked, Is.True);
    }

    [Test]
    public void Schedule_with_canceled_token_throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var subject = new TestScheduler();

        Assert.That(() => subject.Schedule(_ => { }, cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void ScheduleAsync_with_canceled_token_is_canceled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var subject = new TestScheduler();

        Assert.That(async () => await subject.ScheduleAsync(() => { }, cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }
}

[TestFixture]
[TestOf(typeof(AsyncScheduler))]
public class AsyncSchedulerDefaultConstructorTests
{
    private sealed class TestAsyncScheduler : AsyncScheduler
    {
        protected override Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken)
        {
            return scheduled.ExecuteAsync(cancellationToken);
        }
    }

    [Test]
    public void Schedule_invokes_action()
    {
        var subject = new TestAsyncScheduler();
        var invoked = false;

        subject.Schedule(() => invoked = true);

        Assert.That(invoked, Is.True);
    }

    [Test]
    public async Task ScheduleAsync_invokes_action()
    {
        var subject = new TestAsyncScheduler();
        var invoked = false;

        await subject.ScheduleAsync(() => invoked = true, CancellationToken.None);

        Assert.That(invoked, Is.True);
    }
}

[TestFixture]
[TestOf(typeof(DisposableAsyncScheduler))]
public class DisposableAsyncSchedulerDefaultConstructorTests
{
    private sealed class TestDisposableAsyncScheduler : DisposableAsyncScheduler
    {
        protected override Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken)
        {
            return scheduled.ExecuteAsync(cancellationToken);
        }
    }

    [Test]
    public async Task ScheduleAsync_invokes_action()
    {
        var subject = new TestDisposableAsyncScheduler();
        var invoked = false;

        await subject.ScheduleAsync(() => invoked = true, CancellationToken.None);

        Assert.That(invoked, Is.True);
    }
}

#if NET8_0_OR_GREATER

[TestFixture]
[TestOf(typeof(AsyncDisposableAsyncScheduler))]
public class AsyncDisposableAsyncSchedulerDefaultConstructorTests
{
    private sealed class TestAsyncDisposableAsyncScheduler : AsyncDisposableAsyncScheduler
    {
        protected override Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken)
        {
            return scheduled.ExecuteAsync(cancellationToken);
        }
    }

    [Test]
    public async Task ScheduleAsync_invokes_action()
    {
        var subject = new TestAsyncDisposableAsyncScheduler();
        var invoked = false;

        await subject.ScheduleAsync(() => invoked = true, CancellationToken.None);

        Assert.That(invoked, Is.True);
    }
}

#endif
