// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using Josupeit.Practices.Commands;
using Josupeit.Practices.Jobs;
using Josupeit.Practices.Schedulers;
using Josupeit.Practices.Services;
using Josupeit.Practices.Specifications;

// Native AOT smoke test. Each block invokes a library's primary public API so the AOT compiler has
// to compile the path natively; assertions prove the path actually ran rather than silently no-op'd.
// Reaching "AOT SMOKE OK" under a native binary is the gate. Any failure returns a non-zero exit code.
try
{
    // Specifications: compose with the operator overloads and evaluate sync and async.
    Specification<int> spec = new IsPositive() & new IsEven();

    Check(spec.IsSatisfiedBy(4), "specification sync");
    Check(await spec.IsSatisfiedByAsync(4, CancellationToken.None), "specification async");
    Check(!spec.IsSatisfiedBy(3), "specification negative");

    Specification<int> either = new IsPositive() | new IsEven();
    Specification<int> odd = !new IsEven();

    Check(either.IsSatisfiedBy(-2), "specification or");
    Check(await either.IsSatisfiedByAsync(3, CancellationToken.None), "specification or async");
    Check(odd.IsSatisfiedBy(3), "specification not");
    Check(!odd.IsSatisfiedBy(4), "specification not negative");

    // Commands and queries: sync path and the async bridge over the same override.
    new GreetCommand().Execute();
    await new GreetCommand().ExecuteAsync(CancellationToken.None);

    Check(new AnswerQuery().Execute() == 42, "query sync");
    Check(await new AnswerQuery().ExecuteAsync() == 42, "query async bridge");

    // Services: sync and async lifecycle.
    var service = new EchoService();

    service.Start();
    Check(service.State == ServiceState.Running, "service running");
    service.Stop();

    var asyncService = new EchoAsyncService();

    await asyncService.StartAsync();
    await asyncService.StopAsync(CancellationToken.None);

    // TaskPool scheduler: this is the critical path. Scheduling with no explicit ExecutionContext
    // routes through Singletons.DefaultExecutionContext, i.e. the [UnsafeAccessor] field bind.
    var taskPool = new TaskPoolScheduler();
    var counter = 0;

    taskPool.Schedule(() => Interlocked.Increment(ref counter));
    Check(counter == 1, "taskpool sync action");

    await taskPool.ScheduleAsync(ct => Task.CompletedTask);
    Check(taskPool.Schedule(() => 21 * 2) == 42, "taskpool function");
    Check(await taskPool.ScheduleAsync(() => 7) == 7, "taskpool async function");

    // Channel scheduler: async service lifecycle plus channel-backed dispatch.
    var channel = new ChannelScheduler(new ChannelSchedulerOptions { DegreeOfParallelism = 2 });

    await channel.StartAsync();

    var channelCounter = 0;

    await channel.ScheduleAsync(() => Interlocked.Increment(ref channelCounter));
    Check(channelCounter == 1, "channel scheduled work");

    await channel.StopAsync(CancellationToken.None);

    // TaskPool jobs: progress reporting and a result.
    var jobFactory = new TaskPoolJobFactory();
    var job = jobFactory.Create<int, string>(async (progress, ct) =>
    {
        progress.Report(50);

        await Task.Yield();

        return "done";
    });

    job.Start();
    await job;
    Check(job.Result == "done", "taskpool job result");

    // Scheduler-backed jobs: a job factory driven by an IScheduler.
    var schedulerFactory = new SchedulerJobFactory(new TaskPoolScheduler());
    var schedulerJob = schedulerFactory.Create<int>(async ct =>
    {
        await Task.Yield();

        return 99;
    });

    schedulerJob.Start();
    await schedulerJob;
    Check(schedulerJob.Result == 99, "scheduler job result");

    Console.WriteLine("AOT SMOKE OK");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"AOT SMOKE FAILED: {ex}");

    return 1;
}

static void Check(bool condition, string what)
{
    if (!condition)
        throw new InvalidOperationException($"Smoke assertion failed: {what}");
}

file sealed class IsPositive : Specification<int>
{
    public override bool IsSatisfiedBy(int candidate) => candidate > 0;
}

file sealed class IsEven : Specification<int>
{
    public override bool IsSatisfiedBy(int candidate) => candidate % 2 == 0;
}

file sealed class GreetCommand : Command
{
    public override void Execute()
    {
    }
}

file sealed class AnswerQuery : Query<int>
{
    public override int Execute() => 42;
}

file sealed class EchoService : Service
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }
}

file sealed class EchoAsyncService : AsyncService
{
    protected override Task OnStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    protected override Task OnStopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
