// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Jobs.Tests;

[TestOf(typeof(AwaitableExtensions))]
public class AwaitableExtensionsTests
{
    [Test]
    public void GetAwaiter_throws_on_null()
    {
        Assert.That(() => AwaitableExtensions.GetAwaiter(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void GetAwaiter_returns_IJobAwaiter()
    {
        var job = Substitute.For<IJob>();
        var awaiter = job.GetAwaiter();

        Assert.That(awaiter, Is.InstanceOf<IJobAwaiter>());
    }

    [Test]
    public void ConfigureAwait_throws_on_null()
    {
        Assert.That(() => AwaitableExtensions.ConfigureAwait(null!, false), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void ConfigureAwait_returns_ConfiguredJobAwaitable()
    {
        var job = Substitute.For<IJob>();
        var awaitable = job.ConfigureAwait(false);

        Assert.That(awaitable, Is.InstanceOf<ConfiguredJobAwaitable>());
    }

    [Test]
    public void Wait_throws_on_null()
    {
        Assert.That(() => AwaitableExtensions.Wait(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Wait_blocks_until_job_completes()
    {
        var job = Substitute.For<IJob>();
        var started = new ManualResetEventSlim();
        var completed = new ManualResetEventSlim();

        job.IsCompleted.Returns(false);

        var thread = new Thread(() =>
        {
            started.Set();
            job.Wait();
            completed.Set();
        });

        thread.Start();
        started.Wait();

        Assert.That(completed.IsSet, Is.False);

        job.IsCompleted.Returns(true);
        job.Changed += Raise.EventWith(job, EventArgs.Empty);

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        
        thread.Join(TimeSpan.FromSeconds(1));
    }
}
