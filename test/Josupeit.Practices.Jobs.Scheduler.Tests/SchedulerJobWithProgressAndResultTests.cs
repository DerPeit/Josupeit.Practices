// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;
using Josupeit.Practices.Schedulers;

namespace Josupeit.Practices.Jobs.Scheduler.Tests;

[TestOf(typeof(SchedulerJobWithProgressAndResult<,>))]
public class SchedulerJobWithProgressAndResultTests
{
    [Test]
    public void Constructor_throws_on_null_scheduler()
    {
        Assert.That(
            () => new SchedulerJobWithProgressAndResult<string, string>(null!, (_, _) => Task.FromResult("x")),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_throws_on_null_startFunc()
    {
        var scheduler = Substitute.For<IScheduler>();

        Assert.That(
            () => new SchedulerJobWithProgressAndResult<string, string>(scheduler, null!),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Start_completes_successfully()
    {
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (_, _) => Task.FromResult("done"));

        job.Start();

        Assert.That(job.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public void Start_sets_Result_on_success()
    {
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (_, _) => Task.FromResult("result"));

        job.Start();

        Assert.That(job.Result, Is.EqualTo("result"));
    }

    [Test]
    public void Start_faults_on_exception()
    {
        var exception = new TestException();
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (_, _) => Task.FromException<string>(exception));

        job.Start();

        Assert.That(job.IsFaulted, Is.True);
        Assert.That(job.Exception, Is.SameAs(exception));
    }

    [Test]
    public void Start_cancels_when_token_canceled()
    {
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (_, ct) => CanceledTask.From<string>(ct));

        job.Cancel();
        job.Start();

        Assert.That(job.IsCanceled, Is.True);
    }

    [Test]
    public void Start_passes_IProgress_to_delegate()
    {
        IProgress<string>? receivedProgress = null;
        var scheduler = CreateImmediateScheduler();

        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (progress, _) =>
        {
            receivedProgress = progress;

            return Task.FromResult("done");
        });

        job.Start();

        Assert.That(receivedProgress, Is.Not.Null);
    }

    [Test]
    public void Start_faults_when_scheduling_throws()
    {
        var exception = new TestException();
        var scheduler = Substitute.For<IScheduler>();

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(exception));

        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (_, _) => Task.FromResult("x"));

        job.Start();

        Assert.That(job.IsFaulted, Is.True);
        Assert.That(job.IsPending, Is.False);
        Assert.That(job.Exception, Is.InstanceOf<InvalidOperationException>());
        Assert.That(job.Exception!.InnerException, Is.SameAs(exception));
    }

    [Test]
    public void Start_cancels_when_scheduling_is_canceled()
    {
        var scheduler = Substitute.For<IScheduler>();
        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (_, ct) => CanceledTask.From<string>(ct));

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => CanceledTask.From(callInfo.Arg<CancellationToken>()));

        job.Cancel();
        job.Start();

        Assert.That(job.IsCanceled, Is.True);
        Assert.That(job.IsPending, Is.False);
    }

    [Test]
    public void Start_passes_CancellationToken_to_scheduler()
    {
        CancellationToken receivedToken = default;
        var scheduler = Substitute.For<IScheduler>();

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>()).Returns(callInfo =>
        {
            receivedToken = callInfo.Arg<CancellationToken>();

            return callInfo.Arg<Func<Task>>()();
        });

        var job = new SchedulerJobWithProgressAndResult<string, string>(scheduler, (_, _) => Task.FromResult("x"));

        job.Start();

        Assert.That(receivedToken.CanBeCanceled, Is.True);
    }

    private static IScheduler CreateImmediateScheduler()
    {
        var scheduler = Substitute.For<IScheduler>();

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>()).Returns(callInfo => callInfo.Arg<Func<Task>>()());

        return scheduler;
    }

    [Test]
    public void Start_cancels_when_delegate_throws_OperationCanceledException_for_the_job_token()
    {
        var completed = new ManualResetEventSlim();
        var job = new SchedulerJobWithProgressAndResult<int, string>(CreateImmediateScheduler(), (_, ct) => { ct.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)); ct.ThrowIfCancellationRequested(); return Task.FromResult("x"); });

        job.Changed += (_, _) =>
        {
            if (job.IsCompleted)
                completed.Set();
        };

        job.Cancel();
        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(job.IsCanceled, Is.True);
        Assert.That(job.IsFaulted, Is.False);
    }
}
