namespace Josupeit.Practices.Jobs.Tests;

public abstract class JobBaseTests<TJob> where TJob : JobBase
{
    [Test]
    public void Id_is_unique_non_empty_Guid()
    {
        var job1 = CreateJob();
        var job2 = CreateJob();

        Assert.That(job1.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(job2.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(job1.Id, Is.Not.EqualTo(job2.Id));
    }

    [Test]
    public void Initial_state_has_all_flags_false()
    {
        var job = CreateJob();

        Assert.That(job.IsPending, Is.False);
        Assert.That(job.IsExecuting, Is.False);
        Assert.That(job.IsCompleted, Is.False);
        Assert.That(job.IsCompletedSuccessfully, Is.False);
        Assert.That(job.IsFaulted, Is.False);
        Assert.That(job.IsCancelled, Is.False);
        Assert.That(job.Exception, Is.Null);
    }

    [Test]
    public void NotifyPending_sets_IsPending_to_true()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyPending");

        Assert.That(job.IsPending, Is.True);
    }

    [Test]
    public void NotifyPending_raises_Changed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyPending");

        job.Received().InvokeMember("OnChanged");
    }

    [Test]
    public void NotifyExecuting_sets_IsExecuting_to_true_and_IsPending_to_false()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyPending");
        job.InvokeMember("NotifyExecuting");

        Assert.That(job.IsExecuting, Is.True);
        Assert.That(job.IsPending, Is.False);
    }

    [Test]
    public void NotifyExecuting_raises_Changed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyExecuting");

        job.Received().InvokeMember("OnChanged");
    }

    [Test]
    public void NotifyFailed_sets_Exception_IsCompleted_IsFaulted_and_clears_IsExecuting()
    {
        var job = CreateJob();
        var exception = new TestException();

        job.InvokeMember("NotifyExecuting");
        job.InvokeMember("NotifyFailed", exception);

        Assert.That(job.Exception, Is.SameAs(exception));
        Assert.That(job.IsCompleted, Is.True);
        Assert.That(job.IsFaulted, Is.True);
        Assert.That(job.IsExecuting, Is.False);
    }

    [Test]
    public void NotifyFailed_raises_Changed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyFailed", new TestException());

        job.Received().InvokeMember("OnChanged");
    }

    [Test]
    public void NotifyCancelled_sets_IsCancelled_IsCompleted_and_clears_IsPending_IsExecuting()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyPending");
        job.InvokeMember("NotifyCancelled");

        Assert.That(job.IsCancelled, Is.True);
        Assert.That(job.IsCompleted, Is.True);
        Assert.That(job.IsPending, Is.False);
        Assert.That(job.IsExecuting, Is.False);
    }

    [Test]
    public void NotifyCancelled_raises_Changed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyCancelled");

        job.Received().InvokeMember("OnChanged");
    }

    [Test]
    public void ThrowIfCannotStart_does_not_throw_initially()
    {
        var job = CreateJob();

        Assert.That(() => job.InvokeMember("ThrowIfCannotStart"), Throws.Nothing);
    }

    [Test]
    public void ThrowIfCannotStart_throws_when_pending()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyPending");

        Assert.That(() => job.InvokeMember("ThrowIfCannotStart"), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ThrowIfCannotStart_throws_when_executing()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyExecuting");

        Assert.That(() => job.InvokeMember("ThrowIfCannotStart"), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ThrowIfCannotStart_throws_when_completed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyCancelled");

        Assert.That(() => job.InvokeMember("ThrowIfCannotStart"), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ThrowIfCannotCancel_does_not_throw_initially()
    {
        var job = CreateJob();

        Assert.That(() => job.InvokeMember("ThrowIfCannotCancel"), Throws.Nothing);
    }

    [Test]
    public void ThrowIfCannotCancel_throws_when_completed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyCancelled");

        Assert.That(() => job.InvokeMember("ThrowIfCannotCancel"), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void Cancel_does_not_throw_when_not_completed()
    {
        var job = CreateJob();

        Assert.That(() => job.Cancel(), Throws.Nothing);
    }

    [Test]
    public void Cancel_throws_when_completed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyCancelled");

        Assert.That(() => job.Cancel(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void Cancel_requests_cancellation_of_CancellationToken()
    {
        var job = CreateJob();
        var token = (CancellationToken)job.InvokeMember("CancellationToken")!;
        var cancelled = new ManualResetEventSlim();

        token.Register(() => cancelled.Set());
        job.Cancel();

        Assert.That(cancelled.Wait(TimeSpan.FromSeconds(5)), Is.True);
    }

    [Test]
    public void Cancel_is_idempotent()
    {
        var job = CreateJob();

        job.Cancel();

        Assert.That(() => job.Cancel(), Throws.Nothing);
    }

    protected abstract TJob CreateJob();
}
