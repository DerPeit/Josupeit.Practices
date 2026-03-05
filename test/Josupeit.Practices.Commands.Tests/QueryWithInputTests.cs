using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Josupeit.Practices.Commands.Tests;

[TestOf(typeof(Query<,>))]
[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed", Justification = "This does not matter in tests and does not apply to mock setups and such.")]
public class QueryWithInputTests
{
    [Test]
    public void Execute_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var query = Substitute.ForPartsOf<Query<object, object>>();

        Assert.Throws<InvalidOperationException>(() => query.Execute(new()));
    }

    [Test]
    public void ExecuteAsync_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var query = Substitute.ForPartsOf<Query<object, object>>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await query.ExecuteAsync(new()));
    }

    [Test]
    public void Execute_calls_overridden_ExecuteAsync()
    {
        var query = Substitute.ForPartsOf<Query<object, object>>();
        var tcs = new TaskCompletionSource<object>();
        
        tcs.SetResult(new());
        query.ExecuteAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);

        var input = new object();
        var output = query.Execute(input);

        Assert.DoesNotThrow(() => query.Received().ExecuteAsync(input, CancellationToken.None));
        Assert.AreSame(tcs.Task.Result, output);
    }

    [Test]
    public async Task ExecuteAsync_calls_overridden_Execute()
    {
        var query = Substitute.ForPartsOf<Query<object, object>>();
        var input = new object();
        var output = new object();

        query.When(x => x.Execute(Arg.Any<object>())).DoNotCallBase();

        // ReSharper disable once MethodHasAsyncOverload
        query.Execute(Arg.Any<object>()).Returns(output);

        var sameOutput = await query.ExecuteAsync(input);

        Assert.DoesNotThrow(() => query.Received().Execute(input));
        Assert.AreSame(output, sameOutput);
    }

    [Test]
    public void Execute_unwraps_exceptions_from_ExecuteAsync()
    {
        var query = Substitute.ForPartsOf<Query<object, object>>();
        var tcs = new TaskCompletionSource<object>();

        tcs.SetException(new TestException());
        query.ExecuteAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);

        Assert.Throws<TestException>(() => query.Execute(new()));
    }

    [Test]
    public void ExecuteAsync_wraps_exceptions_from_Execute()
    {
        var query = Substitute.ForPartsOf<Query<object, object>>();

        query.When(x => x.Execute(Arg.Any<object>())).Throw<TestException>();

        Task promise = null!;
        
        Assert.DoesNotThrow(() => promise = query.ExecuteAsync(new()));
        Assert.ThrowsAsync<TestException>(async () => await promise);
    }

    [Test]
    public void ExecuteAsync_does_not_call_Execute_when_canceled_early()
    {
        var query = Substitute.ForPartsOf<Query<object, object>>();

        query.When(x => x.Execute(Arg.Any<object>())).DoNotCallBase();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await query.ExecuteAsync(new(), new CancellationToken(canceled: true)));
        Assert.DoesNotThrow(() => query.DidNotReceive().Execute(Arg.Any<object>()));
    }
}