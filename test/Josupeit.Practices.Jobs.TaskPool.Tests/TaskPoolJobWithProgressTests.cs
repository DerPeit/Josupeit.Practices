// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs.TaskPool.Tests;

[TestOf(typeof(TaskPoolJobWithProgress<>))]
public class TaskPoolJobWithProgressTests
{
    [Test]
    public void Constructor_throws_on_null_startFunc()
    {
        Assert.That(() => new TaskPoolJobWithProgress<string>(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Start_completes_successfully()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgress<string>((_, _) => Task.CompletedTask);

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
    public void Start_faults_on_exception()
    {
        var exception = new TestException();
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgress<string>((_, _) => Task.FromException(exception));

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
        var job = new TaskPoolJobWithProgress<string>((_, ct) => Task.FromCanceled(ct));

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

        var job = new TaskPoolJobWithProgress<string>((_, ct) =>
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

    [Test]
    public void Start_passes_IProgress_to_delegate()
    {
        IProgress<string>? receivedProgress = null;
        var completed = new ManualResetEventSlim();

        var job = new TaskPoolJobWithProgress<string>((progress, _) =>
        {
            receivedProgress = progress;

            return Task.CompletedTask;
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
