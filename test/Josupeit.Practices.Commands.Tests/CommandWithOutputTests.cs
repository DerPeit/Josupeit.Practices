// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Commands.Tests;

[TestOf(typeof(CommandWithOutput<>))]
public class CommandWithOutputTests
{
    [Test]
    public void Execute_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var command = Substitute.ForPartsOf<CommandWithOutput<object>>();

        Assert.Throws<InvalidOperationException>(() => command.Execute());
    }

    [Test]
    public void ExecuteAsync_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var command = Substitute.ForPartsOf<CommandWithOutput<object>>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await command.ExecuteAsync());
    }

    [Test]
    public void Execute_calls_overridden_ExecuteAsync()
    {
        var command = Substitute.ForPartsOf<CommandWithOutput<object>>();
        var tcs = new TaskCompletionSource<object>();
        
        tcs.SetResult(new());
        command.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(tcs.Task);

        var output = command.Execute();

        Assert.DoesNotThrow(() => command.Received().ExecuteAsync(CancellationToken.None));
        Assert.AreSame(tcs.Task.Result, output);
    }

    [Test]
    public async Task ExecuteAsync_calls_overridden_Execute()
    {
        var command = Substitute.ForPartsOf<CommandWithOutput<object>>();
        var output = new object();

        command.When(x => x.Execute()).DoNotCallBase();

        // ReSharper disable once MethodHasAsyncOverload
        command.Execute().Returns(output);

        var sameOutput = await command.ExecuteAsync();

        Assert.DoesNotThrow(() => command.Received().Execute());
        Assert.AreSame(output, sameOutput);
    }

    [Test]
    public void Execute_unwraps_exceptions_from_ExecuteAsync()
    {
        var command = Substitute.ForPartsOf<CommandWithOutput<object>>();
        var tcs = new TaskCompletionSource<object>();

        tcs.SetException(new TestException());
        command.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(tcs.Task);

        Assert.Throws<TestException>(() => command.Execute());
    }

    [Test]
    public void ExecuteAsync_wraps_exceptions_from_Execute()
    {
        var command = Substitute.ForPartsOf<CommandWithOutput<object>>();

        command.When(x => x.Execute()).Throw<TestException>();

        Task promise = null!;
        
        Assert.DoesNotThrow(() => promise = command.ExecuteAsync());
        Assert.ThrowsAsync<TestException>(async () => await promise);
    }

    [Test]
    public void ExecuteAsync_does_not_call_Execute_when_canceled_early()
    {
        var command = Substitute.ForPartsOf<CommandWithOutput<object>>();

        command.When(x => x.Execute()).DoNotCallBase();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await command.ExecuteAsync(new CancellationToken(canceled: true)));
        Assert.DoesNotThrow(() => command.DidNotReceive().Execute());
    }
}
