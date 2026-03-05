using System.Threading.Tasks;
using Josupeit.Practices.Schedulers;

namespace Josupeit.Practices.Jobs.Scheduler.Tests;

[TestOf(typeof(SchedulerJobWithProgress<>))]
public class SchedulerJobWithProgressTests
{
    [Test]
    public void Constructor_throws_on_null_scheduler()
    {
        Assert.That(
            () => new SchedulerJobWithProgress<string>(null!, (_, _) => Task.CompletedTask),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_throws_on_null_startFunc()
    {
        var scheduler = Substitute.For<IScheduler>();

        Assert.That(
            () => new SchedulerJobWithProgress<string>(scheduler, null!),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Start_completes_successfully()
    {
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJobWithProgress<string>(scheduler, (_, _) => Task.CompletedTask);

        job.Start();

        Assert.That(job.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public void Start_faults_on_exception()
    {
        var exception = new TestException();
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJobWithProgress<string>(scheduler, (_, _) => Task.FromException(exception));

        job.Start();

        Assert.That(job.IsFaulted, Is.True);
        Assert.That(job.Exception, Is.SameAs(exception));
    }

    [Test]
    public void Start_cancels_when_token_canceled()
    {
        var scheduler = CreateImmediateScheduler();
        var job = new SchedulerJobWithProgress<string>(scheduler, (_, ct) => Task.FromCanceled(ct));

        job.Cancel();
        job.Start();

        Assert.That(job.IsCanceled, Is.True);
    }

    [Test]
    public void Start_passes_IProgress_to_delegate()
    {
        IProgress<string>? receivedProgress = null;
        var scheduler = CreateImmediateScheduler();

        var job = new SchedulerJobWithProgress<string>(scheduler, (progress, _) =>
        {
            receivedProgress = progress;

            return Task.CompletedTask;
        });

        job.Start();

        Assert.That(receivedProgress, Is.Not.Null);
    }

    private static IScheduler CreateImmediateScheduler()
    {
        var scheduler = Substitute.For<IScheduler>();

        scheduler.ScheduleAsync(Arg.Any<Func<Task>>()).Returns(callInfo => callInfo.Arg<Func<Task>>()());

        return scheduler;
    }
}
