// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

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
    public void Start_cancels_when_token_canceled()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgressAndResult<string, string>((_, ct) => CanceledTask.From<string>(ct));

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

    [Test]
    public void Start_throws_synchronously_when_called_twice()
    {
        var gate = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgressAndResult<int, int>((_, _) => { gate.Wait(); return Task.FromResult(1); });

        job.Start();

        try
        {
            Assert.That(() => job.Start(), Throws.InstanceOf<InvalidOperationException>());
        }
        finally
        {
            gate.Set();
        }
    }

    [Test]
    public void Start_cancels_when_delegate_throws_OperationCanceledException_for_the_job_token()
    {
        var completed = new ManualResetEventSlim();
        var job = new TaskPoolJobWithProgressAndResult<int, int>((_, ct) => { ct.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)); ct.ThrowIfCancellationRequested(); return Task.FromResult(1); });

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
