namespace Josupeit.Practices.Jobs.Tests;

[TestOf(typeof(JobWithResult<>))]
public class JobWithResultTests : JobBaseTests<JobWithResult<string>>
{
    [Test]
    public void NotifySucceeded_sets_Result_IsCompleted_and_IsCompletedSuccessfully()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyExecuting");
        job.NotifySucceeded("hello");

        Assert.That(job.Result, Is.EqualTo("hello"));
        Assert.That(job.IsCompleted, Is.True);
        Assert.That(job.IsCompletedSuccessfully, Is.True);
        Assert.That(job.IsExecuting, Is.False);
    }

    [Test]
    public void NotifySucceeded_raises_Changed()
    {
        var job = CreateJob();

        job.NotifySucceeded("result");

        job.Received().InvokeMember("OnChanged");
    }

    [Test]
    public void IHasResult_Result_returns_typed_result()
    {
        var job = CreateJob();

        job.NotifySucceeded("typed");

        Assert.That(((IHasResult)job).Result, Is.EqualTo("typed"));
    }

    protected override JobWithResult<string> CreateJob() => Substitute.ForPartsOf<JobWithResult<string>>();
}
