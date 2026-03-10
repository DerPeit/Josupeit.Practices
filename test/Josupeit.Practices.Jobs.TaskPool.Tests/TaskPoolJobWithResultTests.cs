// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs.TaskPool.Tests;

[TestOf(typeof(TaskPoolJobWithResult<>))]
public class TaskPoolJobWithResultTests
{
    [Test]
    public void Constructor_throws_on_null_startFunc()
    {
        Assert.That(() => new TaskPoolJobWithResult<string>(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Start_completes_successfully()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithResult<string>(_ => Task.FromResult("hello"));

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
    public void Start_sets_Result_on_success()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithResult<string>(_ => Task.FromResult("result"));

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
    public void Start_faults_on_exception()
    {
        var exception = new TestException();
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithResult<string>(_ => Task.FromException<string>(exception));

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
        var job = new TaskPoolJobWithResult<string>(Task.FromCanceled<string>);

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
        var receivedToken = CancellationToken.None;
        var completed = new ManualResetEventSlim();

        var job = new TaskPoolJobWithResult<string>(ct =>
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
}
