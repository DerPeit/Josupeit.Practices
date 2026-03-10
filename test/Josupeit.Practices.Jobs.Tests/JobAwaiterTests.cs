// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Jobs.Tests;

[TestOf(typeof(JobAwaiter))]
public class JobAwaiterTests
{
    [Test]
    public void IsCompleted_delegates_to_job()
    {
        var job = Substitute.For<IJob>();

        job.IsCompleted.Returns(false);

        var awaiter = CreateAwaiter(job);
        
        Assert.That(awaiter.IsCompleted, Is.False);

        job.IsCompleted.Returns(true);
        
        Assert.That(awaiter.IsCompleted, Is.True);
    }

    [Test]
    public void OnCompleted_runs_immediately_when_already_completed()
    {
        var job = Substitute.For<IJob>();
        
        job.IsCompleted.Returns(true);

        var awaiter = CreateAwaiter(job);
        var ran = false;

        awaiter.OnCompleted(() => ran = true);

        Assert.That(ran, Is.True);
    }

    [Test]
    public void OnCompleted_runs_continuation_when_job_completes()
    {
        var job = Substitute.For<IJob>();
        
        job.IsCompleted.Returns(false);

        var awaiter = CreateAwaiter(job);
        var ran = false;

        awaiter.OnCompleted(() => ran = true);
        
        Assert.That(ran, Is.False);

        job.IsCompleted.Returns(true);
        job.Changed += Raise.EventWith(job, EventArgs.Empty);

        Assert.That(ran, Is.True);
    }

    [Test]
    public void OnCompleted_ignores_Changed_when_not_completed()
    {
        var job = Substitute.For<IJob>();
        
        job.IsCompleted.Returns(false);

        var awaiter = CreateAwaiter(job);
        var ran = false;

        awaiter.OnCompleted(() => ran = true);
        job.Changed += Raise.EventWith(job, EventArgs.Empty);

        Assert.That(ran, Is.False);
    }

    [Test]
    public void OnCompleted_throws_on_null_continuation()
    {
        var job = Substitute.For<IJob>();
        var awaiter = CreateAwaiter(job);

        Assert.That(() => awaiter.OnCompleted(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void GetResult_blocks_until_completed()
    {
        var job = Substitute.For<IJob>();
        
        job.IsCompleted.Returns(false);

        var awaiter = CreateAwaiter(job);
        var started = new ManualResetEventSlim();
        var completed = new ManualResetEventSlim();

        var thread = new Thread(() =>
        {
            started.Set();
            awaiter.GetResult();
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

    [Test]
    public void GetResult_returns_immediately_when_already_completed()
    {
        var job = Substitute.For<IJob>();
        
        job.IsCompleted.Returns(true);

        var awaiter = CreateAwaiter(job);

        Assert.That(() => awaiter.GetResult(), Throws.Nothing);
    }

    private static IJobAwaiter CreateAwaiter(IJob job) => job.GetAwaiter();
}
