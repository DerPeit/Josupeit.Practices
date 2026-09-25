// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Diagnostics;
using System.Threading.Tasks;

namespace Josupeit.Practices.Commands.Tests;

[TestOf(typeof(Command))]
public class CommandTests
{
    [Test]
    public void Execute_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var command = Substitute.ForPartsOf<Command>();

        Assert.Throws<InvalidOperationException>(() => command.Execute());
    }

    [Test]
    public void ExecuteAsync_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var command = Substitute.ForPartsOf<Command>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await command.ExecuteAsync());
    }

    [Test]
    public void Execute_calls_overridden_ExecuteAsync()
    {
        var command = Substitute.ForPartsOf<Command>();

        command.When(x => x.ExecuteAsync(Arg.Any<CancellationToken>())).DoNotCallBase();

        command.Execute();

        Assert.DoesNotThrow(() => command.Received().ExecuteAsync(CancellationToken.None));
    }

    [Test]
    public async Task ExecuteAsync_calls_overridden_Execute()
    {
        var command = Substitute.ForPartsOf<Command>();

        command.When(x => x.Execute()).DoNotCallBase();

        await command.ExecuteAsync();

        Assert.DoesNotThrow(() => command.Received().Execute());
    }

    [Test]
    public void Execute_unwraps_exceptions_from_ExecuteAsync()
    {
        var command = Substitute.ForPartsOf<Command>();
        var tcs = new TaskCompletionSource<object>();

        tcs.SetException(new TestException());
        command.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(tcs.Task);

        Assert.Throws<TestException>(() => command.Execute());
    }

    [Test]
    public void ExecuteAsync_wraps_exceptions_from_Execute()
    {
        var command = Substitute.ForPartsOf<Command>();

        command.When(x => x.Execute()).Throw<TestException>();

        Task promise = null!;
        
        Assert.DoesNotThrow(() => promise = command.ExecuteAsync());
        Assert.ThrowsAsync<TestException>(async () => await promise);
    }

    [Test]
    public void ExecuteAsync_does_not_call_Execute_when_canceled_early()
    {
        var command = Substitute.ForPartsOf<Command>();

        command.When(x => x.Execute()).DoNotCallBase();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await command.ExecuteAsync(new CancellationToken(canceled: true)));
        Assert.DoesNotThrow(() => command.DidNotReceive().Execute());
    }

    [Test]
    public void Execute_does_not_report_recursion_when_called_concurrently_on_the_same_instance()
    {
        var started = new CountdownEvent(2);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new GatedCommand(started, gate);
        var first = Task.Run(() => command.Execute());
        var second = Task.Run(() => command.Execute());
        var bothStarted = started.Wait(TimeSpan.FromSeconds(10));

        gate.SetResult(true);

        Func<Task> act = () => Task.WhenAll(first, second);

        Assert.That(bothStarted, Is.True);
        Assert.That(act, Throws.Nothing);
    }

    [Test]
    public void Execute_does_not_accumulate_execution_context_state_across_instances()
    {
        // The recursion guard lives in an AsyncLocal per instance. A guard that leaves its entry behind
        // makes every later execution in the same flow copy a growing map: 20,000 instances then take
        // tens of seconds instead of milliseconds. The bound is generous to stay clear of slow CI machines.
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < 20_000; i++)
            new NoOpCommand().Execute();

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));
    }

    private sealed class NoOpCommand : Command
    {
        public override Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class GatedCommand : Command
    {
        private readonly CountdownEvent _started;
        private readonly TaskCompletionSource<bool> _gate;

        public GatedCommand(CountdownEvent started, TaskCompletionSource<bool> gate)
        {
            _started = started;
            _gate = gate;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _started.Signal();

            await _gate.Task.ConfigureAwait(false);
        }
    }
}
