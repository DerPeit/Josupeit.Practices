// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Jobs.Tests;

[TestOf(typeof(JobWithProgress<>))]
public class JobWithProgressTests : JobBaseTests<JobWithProgress<string>>
{
    [Test]
    public void NotifySucceeded_sets_IsCompleted_and_IsCompletedSuccessfully()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyExecuting");
        job.InvokeMember("NotifySucceeded");

        Assert.That(job.IsCompleted, Is.True);
        Assert.That(job.IsCompletedSuccessfully, Is.True);
        Assert.That(job.IsExecuting, Is.False);
    }

    [Test]
    public void NotifySucceeded_raises_Changed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifySucceeded");

        job.Received().InvokeMember("OnChanged");
    }

    [Test]
    public void NotifyProgress_sets_Progress_and_raises_Changed()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyProgress", "50%");

        Assert.That(job.Progress, Is.EqualTo("50%"));

        job.Received().InvokeMember("OnChanged");
    }

    [Test]
    public void IHasProgress_Progress_returns_typed_progress()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyProgress", "75%");

        Assert.That(((IHasProgress)job).Progress, Is.EqualTo("75%"));
    }

    protected override JobWithProgress<string> CreateJob() => Substitute.ForPartsOf<JobWithProgress<string>>();
}
