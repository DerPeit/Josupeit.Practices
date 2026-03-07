// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Jobs.Tests;

[TestOf(typeof(Job))]
public class JobTests : JobBaseTests<Job>
{
    [Test]
    public void NotifySucceeded_sets_IsCompleted_and_IsCompletedSuccessfully()
    {
        var job = CreateJob();

        job.InvokeMember("NotifyExecuting");
        job.NotifySucceeded();

        Assert.That(job.IsCompleted, Is.True);
        Assert.That(job.IsCompletedSuccessfully, Is.True);
        Assert.That(job.IsExecuting, Is.False);
    }

    [Test]
    public void NotifySucceeded_raises_Changed()
    {
        var job = CreateJob();

        job.NotifySucceeded();

        job.Received().InvokeMember("OnChanged");
    }

    protected override Job CreateJob() => Substitute.ForPartsOf<Job>();
}
