using System.Threading.Tasks;
using Josupeit.Practices.Schedulers;

namespace Josupeit.Practices.Jobs.Scheduler.Tests;

[TestOf(typeof(SchedulerJob))]
public class SchedulerJobTests
{
    [Test]
    public void Constructor_throws_on_null_scheduler()
    {
        Assert.That(
            () => new SchedulerJob(null!, _ => Task.CompletedTask),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_throws_on_null_startFunc()
    {
        var scheduler = Substitute.For<IScheduler>();

        Assert.That(
            () => new SchedulerJob(scheduler, null!),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Start_schedules_on_scheduler()
    {
        var scheduler = Substitute.For<IScheduler>();

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>()).Returns(Task.CompletedTask);

        var job = new SchedulerJob(scheduler, _ => Task.CompletedTask);

        job.Start();

        scheduler.Received(1).ScheduleAsync(Arg.Any<Func<Task>>());
    }

    [Test]
    public void Start_sets_IsPending_before_scheduling()
    {
        var wasPendingDuringSchedule = false;
        var scheduler = Substitute.For<IScheduler>();
        var job = new SchedulerJob(scheduler, _ => Task.CompletedTask);

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>()).Returns(callInfo =>
        {
            wasPendingDuringSchedule = job.IsPending;

            return callInfo.Arg<Func<Task>>()();
        });

        job.Start();

        Assert.That(wasPendingDuringSchedule, Is.True);
    }

    [Test]
    public void Start_completes_successfully()
    {
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJob(scheduler, _ => Task.CompletedTask);

        job.Start();

        Assert.That(job.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public void Start_faults_on_exception()
    {
        var exception = new TestException();
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJob(scheduler, _ => Task.FromException(exception));

        job.Start();

        Assert.That(job.IsFaulted, Is.True);
        Assert.That(job.Exception, Is.SameAs(exception));
    }

    [Test]
    public void Start_cancels_when_token_canceled()
    {
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJob(scheduler, Task.FromCanceled);

        job.Cancel();
        job.Start();

        Assert.That(job.IsCanceled, Is.True);
    }

    private static IScheduler CreateImmediateScheduler()
    {
        var scheduler = Substitute.For<IScheduler>();

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>()).Returns(callInfo => callInfo.Arg<Func<Task>>()());

        return scheduler;
    }
}
