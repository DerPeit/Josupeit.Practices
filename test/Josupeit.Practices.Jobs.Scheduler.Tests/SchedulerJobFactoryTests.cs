// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;
using Josupeit.Practices.Schedulers;

namespace Josupeit.Practices.Jobs.Scheduler.Tests;

[TestOf(typeof(SchedulerJobFactory))]
public class SchedulerJobFactoryTests
{
    [Test]
    public void Constructor_throws_on_null_scheduler()
    {
        Assert.That(() => new SchedulerJobFactory(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Create_with_Func_Task_returns_SchedulerJob()
    {
        var factory = CreateFactory();
        var job = factory.Create(() => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<SchedulerJob>());
    }

    [Test]
    public void Create_with_Func_CancellationToken_Task_returns_SchedulerJob()
    {
        var factory = CreateFactory();
        var job = factory.Create(_ => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<SchedulerJob>());
    }

    [Test]
    public void Create_with_Func_IProgress_Task_returns_SchedulerJobWithProgress()
    {
        var factory = CreateFactory();
        var job = factory.Create<string>(_ => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<SchedulerJobWithProgress<string>>());
    }

    [Test]
    public void Create_with_Func_IProgress_CancellationToken_Task_returns_SchedulerJobWithProgress()
    {
        var factory = CreateFactory();
        var job = factory.Create<string>((_, _) => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<SchedulerJobWithProgress<string>>());
    }

    [Test]
    public void Create_with_Func_Task_TResult_returns_SchedulerJobWithResult()
    {
        var factory = CreateFactory();
        var job = factory.Create(() => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<SchedulerJobWithResult<string>>());
    }

    [Test]
    public void Create_with_Func_CancellationToken_Task_TResult_returns_SchedulerJobWithResult()
    {
        var factory = CreateFactory();
        var job = factory.Create(_ => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<SchedulerJobWithResult<string>>());
    }

    [Test]
    public void Create_with_Func_IProgress_Task_TResult_returns_SchedulerJobWithProgressAndResult()
    {
        var factory = CreateFactory();
        var job = factory.Create<string, string>(_ => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<SchedulerJobWithProgressAndResult<string, string>>());
    }

    [Test]
    public void Create_with_Func_IProgress_CancellationToken_Task_TResult_returns_SchedulerJobWithProgressAndResult()
    {
        var factory = CreateFactory();
        var job = factory.Create<string, string>((_, _) => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<SchedulerJobWithProgressAndResult<string, string>>());
    }

    private static SchedulerJobFactory CreateFactory()
    {
        return new SchedulerJobFactory(Substitute.For<IScheduler>());
    }
}
