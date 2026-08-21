// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(ScheduledNonCancellableSyncAction))]
public class ScheduledNonCancellableSyncActionTests
{
    [Test]
    public void Decline_cancels_wait_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        subject.Decline();

        Assert.That(subject.Wait, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Decline_cancels_promise_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        subject.Decline();

        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_with_canceled_token_cancels_wait_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(subject.Wait, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_with_canceled_token_cancels_promise_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_invokes_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);

        callback.Received(1)();
    }

    [Test]
    public void Execute_restores_execution_context([ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod)
    {
        var sentinelValue = new object();
        var sentinel1 = new AsyncLocal<object> { Value = sentinelValue };
        var sentinel2 = new AsyncLocal<object>();

        var callback = Substitute.For<Action>();
        var ec = ExecutionContext.Capture();

        sentinel2.Value = sentinelValue;

        var subject = new ScheduledNonCancellableSyncAction(callback, ec);
        using var e = new ManualResetEventSlim(initialState: false);

        callback
            .When(x => x())
            .Do(_ =>
            {
                Assert.That(sentinel1.Value, Is.SameAs(sentinelValue));
                Assert.That(sentinel2.Value, Is.Null);
            });

        ThreadPool.UnsafeQueueUserWorkItem(
            static state =>
            {
                var subject = (IScheduled)((object[])state!)[0];
                var executeMethod = (MethodInfo)((object[])state)[1];
                var sentinel = (AsyncLocal<object>)((object[])state)[2];
                var e = (ManualResetEventSlim)((object[])state)[3];

                Assert.That(sentinel.Value, Is.Null);

                switch (Execute(subject, executeMethod, CancellationToken.None))
                {
                    case Action execute:
                        execute();
                        e.Set();
                        break;

                    case Func<Task> executeAsync:
                        executeAsync().ContinueWith(_ => e.Set(), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                        break;
                }
            },
            new object[] { subject, executeMethod, sentinel1, e });

        Assert.That(e.Wait(TimeSpan.FromSeconds(30)), Is.True);
        callback.Received(1)();
    }

    [Test]
    public async Task Execute_does_not_leak_different_execution_context([ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod)
    {
        var sentinelValue = new object();
        var sentinel = new AsyncLocal<object> { Value = sentinelValue };

        var callback = Substitute.For<Action>();
        
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext: null);
        using var e = new ManualResetEventSlim(initialState: false);

        callback
            .When(x => x())
            .Do(_ =>
            {
                Assert.That(sentinel.Value, Is.Null);
            });

        switch (Execute(subject, executeMethod, CancellationToken.None))
        {
            case Action execute:
                execute();
                break;

            case Func<Task> executeAsync:
                await executeAsync();
                break;
        }

        Assert.That(sentinel.Value, Is.SameAs(sentinelValue));

        callback.Received(1)();
    }

    [Test]
    public async Task Execute_unblocks_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        switch (Execute(subject, executeMethod, CancellationToken.None))
        {
            case Action execute:
                execute();
                break;

            case Func<Task> executeAsync:
                await executeAsync();
                break;
        }

        Assert.That(subject.Wait, Throws.Nothing);
    }


    [Test]
    public async Task Execute_unblocks_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        switch (Execute(subject, executeMethod, CancellationToken.None))
        {
            case Action execute:
                execute();
                break;

            case Func<Task> executeAsync:
                await executeAsync();
                break;
        }

        await subject.Promise;
        Assert.That(async () => await subject.Promise, Throws.Nothing);
    }

    [Test]
    public void Cancellation_with_execute_token_declines_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback
            .When(x => x())
            .Do(_ =>
            {
                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
                proposedCancellationToken.ThrowIfCancellationRequested();
            });

        Assert.That(Execute(subject, executeMethod, proposedCancellationToken), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(subject.Wait, Throws.TypeOf<ExecutionDeclinedException>());
    }

    [Test]
    public void Cancellation_with_execute_token_declines_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback
            .When(x => x())
            .Do(_ =>
            {
                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
                proposedCancellationToken.ThrowIfCancellationRequested();
            });

        Assert.That(Execute(subject, executeMethod, proposedCancellationToken), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());
    }

    [Test]
    public void Arbitrary_other_cancellation_cancels_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod, 
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback
            .When(x => x())
            .Do(_ =>
            {
                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
                unknownCancellationToken.ThrowIfCancellationRequested();
            });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(subject.Wait, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Arbitrary_other_cancellation_cancels_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback
            .When(x => x())
            .Do(_ =>
            {
                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
                unknownCancellationToken.ThrowIfCancellationRequested();
            });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Exception_in_callback_is_being_unwrapped_by_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        callback.When(x => x()).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(subject.Wait, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Exception_in_callback_is_being_unwrapped_by_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        callback.When(x => x()).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void IsCancellationRequested_returns_false([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext);

        Assert.That(((IScheduled)subject).IsCancellationRequested, Is.False);
    }

    [Test]
    public void TaskCompletionSource_runs_continuations_asynchronously()
    {
        var callback = Substitute.For<Action>();
        var subject = new ScheduledNonCancellableSyncAction(callback, executionContext: null);

        var tcsField = subject.GetType().GetField("_taskCompletionSource", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var tcs = (TaskCompletionSource<object?>)tcsField.GetValue(subject)!;

        Assert.That(tcs.Task.CreationOptions & TaskCreationOptions.RunContinuationsAsynchronously, Is.EqualTo(TaskCreationOptions.RunContinuationsAsynchronously));
    }

    private static Delegate Execute(IScheduled subject, MethodInfo mi, CancellationToken cancellationToken)
    {
        var call = Expression.Call(Expression.Constant(subject), mi, Expression.Constant(cancellationToken));

        if (mi.Name == nameof(IScheduled.Execute))
        {
            return Expression.Lambda<Action>(call).Compile();
        }

        return Expression.Lambda<Func<Task>>(call).Compile();
    }

    private static IEnumerable<MethodInfo> ReturnExecuteThenExecuteAsync()
    {
        yield return typeof(IScheduled).GetMethod(nameof(IScheduled.Execute))!;
        yield return typeof(IScheduled).GetMethod(nameof(IScheduled.ExecuteAsync))!;
    }

    private static IEnumerable<ExecutionContext?> ReturnCapturedContextThenNull()
    {
        yield return ExecutionContext.Capture();
        yield return null;
    }

    private sealed class TestException : Exception;
}
