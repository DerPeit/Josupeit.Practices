using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs.TaskPool.Tests;

[TestOf(typeof(TaskPoolJobWithProgressAndResult<,>))]
public class TaskPoolJobWithProgressAndResultTests
{
    [Test]
    public void Constructor_throws_on_null_startFunc()
    {
        Assert.That(
            () => new TaskPoolJobWithProgressAndResult<string, string>(null!),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    [CancelAfter(5000)]
    public void Start_completes_successfully()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgressAndResult<string, string>((_, _) => Task.FromResult("done"));

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
    [CancelAfter(5000)]
    public void Start_sets_Result_on_success()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgressAndResult<string, string>((_, _) => Task.FromResult("result"));

        job.Changed += (_, _) =>
        {
            if (job.IsCompleted)
                completed.Set();
        };

        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(job.Result, Is.EqualTo("result"));
    }

    [Test]
    [CancelAfter(5000)]
    public void Start_faults_on_exception()
    {
        var exception = new TestException();
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgressAndResult<string, string>((_, _) => Task.FromException<string>(exception));

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
    [CancelAfter(5000)]
    public void Start_cancels_when_token_canceled()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgressAndResult<string, string>((_, ct) => Task.FromCanceled<string>(ct));

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
    [CancelAfter(5000)]
    public void Start_passes_CancellationToken_to_delegate()
    {
        CancellationToken receivedToken = default;
        var completed = new ManualResetEventSlim();

        var job = new TaskPoolJobWithProgressAndResult<string, string>((_, ct) =>
        {
            receivedToken = ct;

            return Task.FromResult("done");
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

    [Test]
    [CancelAfter(5000)]
    public void Start_passes_IProgress_to_delegate()
    {
        IProgress<string>? receivedProgress = null;
        var completed = new ManualResetEventSlim();

        var job = new TaskPoolJobWithProgressAndResult<string, string>((progress, _) =>
        {
            receivedProgress = progress;

            return Task.FromResult("done");
        });

        job.Changed += (_, _) =>
        {
            if (job.IsCompleted)
                completed.Set();
        };

        job.Start();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(receivedProgress, Is.Not.Null);
    }
}
