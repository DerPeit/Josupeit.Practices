// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers.TaskPool.Tests;

[TestFixture]
[TestOf(typeof(TaskPoolScheduler))]
public class TaskPoolSchedulerTests
{
    [Test]
    public void Schedule_executes_callback()
    {
        var scheduler = new TaskPoolScheduler();
        var executed = false;

        scheduler.Schedule(() => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public void Schedule_executes_callback_on_a_thread_pool_thread()
    {
        var scheduler = new TaskPoolScheduler();
        var isThreadPoolThread = false;
        var callbackThreadId = -1;

        scheduler.Schedule(() =>
        {
            isThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
        });

        Assert.That(isThreadPoolThread, Is.True);
        Assert.That(callbackThreadId, Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId));
    }

    [Test]
    public async Task ScheduleAsync_executes_callback()
    {
        var scheduler = new TaskPoolScheduler();
        var executed = false;

        await scheduler.ScheduleAsync(() => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public void Schedule_propagates_callback_exception_but_scheduler_remains_operational()
    {
        var scheduler = new TaskPoolScheduler();

        Assert.That(() => scheduler.Schedule(() => throw new TestException()), Throws.InstanceOf<TestException>());

        var executed = false;

        scheduler.Schedule(() => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public void Schedule_preserves_execution_context()
    {
        var scheduler = new TaskPoolScheduler();

        var asyncLocal = new AsyncLocal<string>
        {
            Value = "preserved"
        };

        string? observedValue = null;

        scheduler.Schedule(() => observedValue = asyncLocal.Value);

        Assert.That(observedValue, Is.EqualTo("preserved"));
    }

    [Test]
    public void UnsafeSchedule_does_not_preserve_execution_context()
    {
        var scheduler = new TaskPoolScheduler();

        var asyncLocal = new AsyncLocal<string>
        {
            Value = "should-not-flow"
        };

        var observedValue = "initial";

        scheduler.UnsafeSchedule(() => observedValue = asyncLocal.Value);

        Assert.That(observedValue, Is.Null);
    }
}
