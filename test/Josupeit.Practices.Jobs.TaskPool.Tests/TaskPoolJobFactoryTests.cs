using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs.TaskPool.Tests;

[TestOf(typeof(TaskPoolJobFactory))]
public class TaskPoolJobFactoryTests
{
    [Test]
    public void Create_with_Func_Task_returns_TaskPoolJob()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create(() => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<TaskPoolJob>());
    }

    [Test]
    public void Create_with_Func_CancellationToken_Task_returns_TaskPoolJob()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create(_ => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<TaskPoolJob>());
    }

    [Test]
    public void Create_with_Func_IProgress_Task_returns_TaskPoolJobWithProgress()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create<string>(_ => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<TaskPoolJobWithProgress<string>>());
    }

    [Test]
    public void Create_with_Func_IProgress_CancellationToken_Task_returns_TaskPoolJobWithProgress()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create<string>((_, _) => Task.CompletedTask);

        Assert.That(job, Is.InstanceOf<TaskPoolJobWithProgress<string>>());
    }

    [Test]
    public void Create_with_Func_Task_TResult_returns_TaskPoolJobWithResult()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create(() => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<TaskPoolJobWithResult<string>>());
    }

    [Test]
    public void Create_with_Func_CancellationToken_Task_TResult_returns_TaskPoolJobWithResult()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create(_ => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<TaskPoolJobWithResult<string>>());
    }

    [Test]
    public void Create_with_Func_IProgress_Task_TResult_returns_TaskPoolJobWithProgressAndResult()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create<string, string>(_ => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<TaskPoolJobWithProgressAndResult<string, string>>());
    }

    [Test]
    public void Create_with_Func_IProgress_CancellationToken_Task_TResult_returns_TaskPoolJobWithProgressAndResult()
    {
        var factory = new TaskPoolJobFactory();
        var job = factory.Create<string, string>((_, _) => Task.FromResult("result"));

        Assert.That(job, Is.InstanceOf<TaskPoolJobWithProgressAndResult<string, string>>());
    }
}
