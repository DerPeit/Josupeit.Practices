// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Josupeit.Practices.Commands.Tests;

[TestOf(typeof(Query<>))]
[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed", Justification = "This does not matter in tests and does not apply to mock setups and such.")]
public class QueryTests
{
    [Test]
    public void Execute_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var query = Substitute.ForPartsOf<Query<object>>();

        Assert.Throws<InvalidOperationException>(() => query.Execute());
    }

    [Test]
    public void ExecuteAsync_fails_if_neither_Execute_nor_ExecuteAsync_were_overridden()
    {
        var query = Substitute.ForPartsOf<Query<object>>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await query.ExecuteAsync());
    }

    [Test]
    public void Execute_calls_overridden_ExecuteAsync()
    {
        var query = Substitute.ForPartsOf<Query<object>>();
        var tcs = new TaskCompletionSource<object>();
        
        tcs.SetResult(new());
        query.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(tcs.Task);

        var output = query.Execute();

        Assert.DoesNotThrow(() => query.Received().ExecuteAsync(CancellationToken.None));
        Assert.AreSame(tcs.Task.Result, output);
    }

    [Test]
    public async Task ExecuteAsync_calls_overridden_Execute()
    {
        var query = Substitute.ForPartsOf<Query<object>>();
        var output = new object();

        query.When(x => x.Execute()).DoNotCallBase();

        // ReSharper disable once MethodHasAsyncOverload
        query.Execute().Returns(output);

        var sameOutput = await query.ExecuteAsync();

        Assert.DoesNotThrow(() => query.Received().Execute());
        Assert.AreSame(output, sameOutput);
    }

    [Test]
    public void Execute_unwraps_exceptions_from_ExecuteAsync()
    {
        var query = Substitute.ForPartsOf<Query<object>>();
        var tcs = new TaskCompletionSource<object>();

        tcs.SetException(new TestException());
        query.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(tcs.Task);

        Assert.Throws<TestException>(() => query.Execute());
    }

    [Test]
    public void ExecuteAsync_wraps_exceptions_from_Execute()
    {
        var query = Substitute.ForPartsOf<Query<object>>();

        query.When(x => x.Execute()).Throw<TestException>();

        Task promise = null!;
        
        Assert.DoesNotThrow(() => promise = query.ExecuteAsync());
        Assert.ThrowsAsync<TestException>(async () => await promise);
    }

    [Test]
    public void ExecuteAsync_does_not_call_Execute_when_canceled_early()
    {
        var query = Substitute.ForPartsOf<Query<object>>();

        query.When(x => x.Execute()).DoNotCallBase();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await query.ExecuteAsync(new CancellationToken(canceled: true)));
        Assert.DoesNotThrow(() => query.DidNotReceive().Execute());
    }
}
