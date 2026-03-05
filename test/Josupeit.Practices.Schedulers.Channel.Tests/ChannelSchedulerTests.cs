using System.Threading.Tasks;
using Josupeit.Practices.Services;

namespace Josupeit.Practices.Schedulers.Channel.Tests;

[TestFixture]
[TestOf(typeof(ChannelScheduler))]
public class ChannelSchedulerTests
{
    protected int DegreeOfParallelism = 1;
    protected TimeSpan GracefulStopDelay = TimeSpan.FromSeconds(3);

    protected ChannelScheduler Subject = null!;

    [SetUp]
    public async Task SetUp()
    {
        var options = new ChannelSchedulerOptions
        {
            DegreeOfParallelism = DegreeOfParallelism,
            GracefulStopDelay = GracefulStopDelay,
        };

        Subject = new ChannelScheduler(options);

        await Subject.StartAsync(CancellationToken.None);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (Subject.State is ServiceState.Running or ServiceState.Paused)
            await Subject.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Schedule_executes_callback()
    {
        var tcs = new TaskCompletionSource<object?>();

        await Subject.ScheduleAsync(() => tcs.TrySetResult(null));

        Assert.That(tcs.Task.IsCompleted, Is.True);
    }

    [Test]
    public async Task ScheduleAsync_executes_callback()
    {
        var executed = false;

        await Subject.ScheduleAsync(() => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public async Task Schedule_does_not_crash_if_callback_throws()
    {
        Assert.That(
            async () => await Subject.ScheduleAsync(() => throw new TestException()),
            Throws.InstanceOf<TestException>());

        var executed = false;

        await Subject.ScheduleAsync(() => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public async Task Schedule_preserves_execution_context()
    {
        var asyncLocal = new AsyncLocal<string>
        {
            Value = "preserved"
        };

        string? observedValue = null;

        await Subject.ScheduleAsync(() => observedValue = asyncLocal.Value);

        Assert.That(observedValue, Is.EqualTo("preserved"));
    }

    [Test]
    public async Task UnsafeSchedule_does_not_preserve_execution_context()
    {
        var asyncLocal = new AsyncLocal<string>
        {
            Value = "should-not-flow"
        };

        var observedValue = "initial";

        await Subject.UnsafeScheduleAsync(() => observedValue = asyncLocal.Value);

        Assert.That(observedValue, Is.Null);
    }

    [Test]
    public void ScheduleAsync_respects_cancellation_token()
    {
        using var cts = new CancellationTokenSource();

        cts.Cancel();

        Assert.That(
            async () => await Subject.ScheduleAsync(() => { }, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [TestFixture(1)]
    [TestFixture(2)]
    [TestFixture(4)]
    [TestOf(typeof(ChannelScheduler))]
    public class Concurrency : ChannelSchedulerTests
    {
        private readonly int _dop;

        public Concurrency(int dop)
        {
            _dop = dop;
            DegreeOfParallelism = dop;
        }

        [Test]
        public async Task Schedule_does_not_exceed_degree_of_parallelism()
        {
            var gate = new SemaphoreSlim(0);
            var concurrentCount = 0;
            var maxConcurrent = 0;
            var itemCount = _dop + 1;
            var completed = new TaskCompletionSource<object?>();

            var scheduleTasks = new Task[itemCount];

            for (var i = 0; i < itemCount; i++)
            {
                scheduleTasks[i] = Subject.ScheduleAsync(async () =>
                {
                    var current = Interlocked.Increment(ref concurrentCount);

                    lock (completed)
                    {
                        if (current > maxConcurrent)
                            maxConcurrent = current;
                    }

                    await gate.WaitAsync();

                    Interlocked.Decrement(ref concurrentCount);
                });
            }

            // Wait briefly for all items to start processing
            await Task.Delay(200);

            // Release all gates
            gate.Release(itemCount);

            await Task.WhenAll(scheduleTasks);

            Assert.That(maxConcurrent, Is.LessThanOrEqualTo(_dop));
        }

        [Test]
        public async Task Schedule_utilizes_degree_of_parallelism()
        {
            var gate = new SemaphoreSlim(0);
            var concurrentCount = 0;
            var allRunning = new TaskCompletionSource<object?>();

            var scheduleTasks = new Task[_dop];

            for (var i = 0; i < _dop; i++)
            {
                scheduleTasks[i] = Subject.ScheduleAsync(async () =>
                {
                    var current = Interlocked.Increment(ref concurrentCount);

                    if (current == _dop)
                        allRunning.TrySetResult(null);

                    await gate.WaitAsync();
                });
            }

            await allRunning.Task;

            Assert.That(Volatile.Read(ref concurrentCount), Is.EqualTo(_dop));

            gate.Release(_dop);

            await Task.WhenAll(scheduleTasks);
        }
    }

    [TestFixture]
    [TestOf(typeof(ChannelScheduler))]
    public class GracefulShutdown : ChannelSchedulerTests
    {
        [Test]
        public async Task StopAsync_waits_for_running_work()
        {
            var workStarted = new TaskCompletionSource<object?>();
            var workCanFinish = new TaskCompletionSource<object?>();
            var workCompleted = false;

            var scheduleTask = Subject.ScheduleAsync(async () =>
            {
                workStarted.TrySetResult(null);
                await workCanFinish.Task;
                workCompleted = true;
            });

            await workStarted.Task;

            var stopTask = Subject.StopAsync(CancellationToken.None);

            // Work is still running, stop should not have completed yet
            await Task.Delay(100);

            Assert.That(stopTask.IsCompleted, Is.False);

            workCanFinish.TrySetResult(null);

            await stopTask;
            await scheduleTask;

            Assert.That(workCompleted, Is.True);
        }

        [Test]
        public async Task StopAsync_declines_pending_work()
        {
            var gate = new TaskCompletionSource<object?>();

            // Fill the single consumer with blocking work
            var blockingTask = Subject.ScheduleAsync(async () => await gate.Task);

            // Enqueue a second item that will be pending
            var pendingTask = Subject.ScheduleAsync(() => { });

            // Stop the scheduler — this closes the channel and declines pending items
            var stopTask = Subject.StopAsync(CancellationToken.None);

            // Release the blocking work so the consumer can finish
            gate.TrySetResult(null);

            await stopTask;
            await blockingTask;

            Assert.That(async () => await pendingTask, Throws.InstanceOf<ExecutionDeclinedException>());
        }
    }

    [TestFixture]
    [TestOf(typeof(ChannelScheduler))]
    public class WithZeroGracefulStopDelay : ChannelSchedulerTests
    {
        public WithZeroGracefulStopDelay()
        {
            GracefulStopDelay = TimeSpan.Zero;
        }

        [Test]
        public async Task StopAsync_cancels_running_work_immediately()
        {
            var workStarted = new TaskCompletionSource<object?>();
            var observedToken = CancellationToken.None;

            _ = Subject.ScheduleAsync(async ct =>
            {
                observedToken = ct;
                workStarted.TrySetResult(null);

                // Block until canceled
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            });

            await workStarted.Task;

            await Subject.StopAsync(CancellationToken.None);

            Assert.That(observedToken.IsCancellationRequested, Is.True);
        }
    }

    [TestFixture]
    [TestOf(typeof(ChannelScheduler))]
    public class WithNegativeGracefulStopDelay : ChannelSchedulerTests
    {
        public WithNegativeGracefulStopDelay()
        {
            GracefulStopDelay = TimeSpan.FromMilliseconds(-1);
        }

        [Test]
        public async Task StopAsync_waits_indefinitely_for_running_work()
        {
            var workStarted = new TaskCompletionSource<object?>();
            var workCanFinish = new TaskCompletionSource<object?>();
            var workCompleted = false;

            var scheduleTask = Subject.ScheduleAsync(async () =>
            {
                workStarted.TrySetResult(null);
                await workCanFinish.Task;
                workCompleted = true;
            });

            await workStarted.Task;

            var stopTask = Subject.StopAsync(CancellationToken.None);

            await Task.Delay(100);

            Assert.That(stopTask.IsCompleted, Is.False);

            workCanFinish.TrySetResult(null);

            await stopTask;
            await scheduleTask;

            Assert.That(workCompleted, Is.True);
        }
    }

    [TestFixture]
    [TestOf(typeof(ChannelScheduler))]
    public class WithPositiveGracefulStopDelay : ChannelSchedulerTests
    {
        public WithPositiveGracefulStopDelay()
        {
            GracefulStopDelay = TimeSpan.FromMilliseconds(200);
        }

        [Test]
        public async Task StopAsync_cancels_running_work_after_timeout()
        {
            var workStarted = new TaskCompletionSource<object?>();
            var observedToken = CancellationToken.None;

            _ = Subject.ScheduleAsync(async ct =>
            {
                observedToken = ct;
                workStarted.TrySetResult(null);

                // Block until canceled
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            });

            await workStarted.Task;

            await Subject.StopAsync(CancellationToken.None);

            Assert.That(observedToken.IsCancellationRequested, Is.True);
        }
    }

    [TestFixture]
    [TestOf(typeof(ChannelScheduler))]
    public class PauseResume : ChannelSchedulerTests
    {
        [Test]
        public async Task PauseAsync_blocks_processing()
        {
            await Subject.PauseAsync(CancellationToken.None);

            var executed = false;
            var scheduleTask = Subject.ScheduleAsync(() => executed = true);

            await Task.Delay(200);

            Assert.That(executed, Is.False);

            await Subject.ResumeAsync(CancellationToken.None);

            await scheduleTask;

            Assert.That(executed, Is.True);
        }

        [Test]
        public async Task ResumeAsync_unblocks_processing()
        {
            await Subject.PauseAsync(CancellationToken.None);

            var tcs = new TaskCompletionSource<object?>();

            _ = Subject.ScheduleAsync(() => tcs.TrySetResult(null));

            await Subject.ResumeAsync(CancellationToken.None);

            await tcs.Task;

            Assert.That(tcs.Task.IsCompleted, Is.True);
        }

        [Test]
        public async Task StopAsync_while_paused_unblocks_consumers()
        {
            await Subject.PauseAsync(CancellationToken.None);

            await Subject.StopAsync(CancellationToken.None);

            Assert.That(Subject.State, Is.EqualTo(ServiceState.Stopped));
        }
    }
}
