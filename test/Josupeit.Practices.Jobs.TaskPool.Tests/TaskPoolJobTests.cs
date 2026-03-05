using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs.TaskPool.Tests;

[TestOf(typeof(TaskPoolJob))]
public class TaskPoolJobTests
{
    [Test]
    public void Constructor_throws_on_null_startFunc()
    {
        Assert.That(() => new TaskPoolJob(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Start_completes_successfully()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJob(_ => Task.CompletedTask);

        job.Changed += (_, _) =>
        {
            if (job.IsCompleted)
                completed.Set();
        };

        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(job.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public void Start_transitions_through_pending_and_executing()
    {
        var wasPending = false;
        var wasExecuting = false;
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJob(_ => Task.CompletedTask);

        job.Changed += (_, _) =>
        {
            if (job.IsPending) wasPending = true;
            if (job.IsExecuting) wasExecuting = true;
            if (job.IsCompleted) completed.Set();
        };

        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(wasPending, Is.True);
        Assert.That(wasExecuting, Is.True);
    }

    [Test]
    public void Start_faults_on_exception()
    {
        var exception = new TestException();
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJob(_ => Task.FromException(exception));

        job.Changed += (_, _) =>
        {
            if (job.IsCompleted)
                completed.Set();
        };

        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(job.IsFaulted, Is.True);
        Assert.That(job.Exception, Is.SameAs(exception));
    }

    [Test]
    public void Start_cancels_when_token_canceled()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJob(Task.FromCanceled);

        job.Changed += (_, _) =>
        {
            if (job.IsCompleted)
                completed.Set();
        };

        job.Cancel();
        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(job.IsCanceled, Is.True);
    }

    [Test]
    public void Start_passes_CancellationToken_to_delegate()
    {
        CancellationToken receivedToken = default;
        var completed = new ManualResetEventSlim();

        var job = new TaskPoolJob(ct =>
        {
            receivedToken = ct;

            return Task.CompletedTask;
        });

        job.Changed += (_, _) =>
        {
            if (job.IsCompleted)
                completed.Set();
        };

        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(receivedToken.CanBeCanceled, Is.True);
    }
}
