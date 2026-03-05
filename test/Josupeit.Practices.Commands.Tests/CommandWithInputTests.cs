using System.Threading.Tasks;

namespace Josupeit.Practices.Commands.Tests;

[TestOf(typeof(Command<>))]
public class CommandWithInputTests
{
    [Test]
    public void Execute_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var command = Substitute.ForPartsOf<Command<object>>();

        Assert.Throws<InvalidOperationException>(() => command.Execute(new()));
    }

    [Test]
    public void ExecuteAsync_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var command = Substitute.ForPartsOf<Command<object>>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await command.ExecuteAsync(new()));
    }

    [Test]
    public void Execute_calls_overridden_ExecuteAsync()
    {
        var command = Substitute.ForPartsOf<Command<object>>();

        command.When(x => x.ExecuteAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())).DoNotCallBase();

        var input = new object();

        command.Execute(input);

        Assert.DoesNotThrow(() => command.Received().ExecuteAsync(input, CancellationToken.None));
    }

    [Test]
    public async Task ExecuteAsync_calls_overridden_Execute()
    {
        var command = Substitute.ForPartsOf<Command<object>>();

        command.When(x => x.Execute(Arg.Any<object>())).DoNotCallBase();

        var input = new object();

        await command.ExecuteAsync(input);

        Assert.DoesNotThrow(() => command.Received().Execute(input));
    }

    [Test]
    public void Execute_unwraps_exceptions_from_ExecuteAsync()
    {
        var command = Substitute.ForPartsOf<Command<object>>();
        var tcs = new TaskCompletionSource<object>();

        tcs.SetException(new TestException());
        command.ExecuteAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);

        Assert.Throws<TestException>(() => command.Execute(new()));
    }

    [Test]
    public void ExecuteAsync_wraps_exceptions_from_Execute()
    {
        var command = Substitute.ForPartsOf<Command<object>>();

        command.When(x => x.Execute(Arg.Any<object>())).Throw<TestException>();

        Task promise = null!;
        
        Assert.DoesNotThrow(() => promise = command.ExecuteAsync(new()));
        Assert.ThrowsAsync<TestException>(async () => await promise);
    }

    [Test]
    public void ExecuteAsync_does_not_call_Execute_when_canceled_early()
    {
        var command = Substitute.ForPartsOf<Command<object>>();

        command.When(x => x.Execute(Arg.Any<object>())).DoNotCallBase();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await command.ExecuteAsync(new(), new CancellationToken(canceled: true)));
        Assert.DoesNotThrow(() => command.DidNotReceive().Execute(Arg.Any<object>()));
    }
}