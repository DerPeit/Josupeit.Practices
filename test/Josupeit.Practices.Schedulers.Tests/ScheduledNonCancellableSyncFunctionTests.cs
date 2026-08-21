// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(ScheduledNonCancellableSyncFunction<>))]
public class ScheduledNonCancellableSyncFunctionTests
{
    private const int SENTINEL_RESULT = 42;

    [Test]
    public void Decline_cancels_wait_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        subject.Decline();

        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Decline_cancels_promise_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        subject.Decline();

        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_with_canceled_token_cancels_wait_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_with_canceled_token_cancels_promise_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_invokes_callback_and_returns_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        callback().Returns(SENTINEL_RESULT);
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(subject.WaitAndUnwrap(), Is.EqualTo(SENTINEL_RESULT));

        callback.Received(1)();
    }

    [Test]
    public void Execute_restores_execution_context([ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod)
    {
        var sentinelValue = new object();
        var sentinel1 = new AsyncLocal<object> { Value = sentinelValue };
        var sentinel2 = new AsyncLocal<object>();

        var callback = Substitute.For<Func<int>>();
        var ec = ExecutionContext.Capture();

        sentinel2.Value = sentinelValue;

        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, ec);
        using var e = new ManualResetEventSlim(initialState: false);

        callback().Returns(_ =>
        {
            Assert.That(sentinel1.Value, Is.SameAs(sentinelValue));
            Assert.That(sentinel2.Value, Is.Null);
            return SENTINEL_RESULT;
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

        e.Wait();
        callback.Received(1)();
    }

    [Test]
    public async Task Execute_does_not_leak_different_execution_context([ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod)
    {
        var sentinelValue = new object();
        var sentinel = new AsyncLocal<object> { Value = sentinelValue };

        var callback = Substitute.For<Func<int>>();

        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext: null);

        callback().Returns(_ =>
        {
            Assert.That(sentinel.Value, Is.Null);
            return SENTINEL_RESULT;
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
    public async Task Execute_unblocks_wait_with_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        callback().Returns(SENTINEL_RESULT);
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        switch (Execute(subject, executeMethod, CancellationToken.None))
        {
            case Action execute:
                execute();
                break;

            case Func<Task> executeAsync:
                await executeAsync();
                break;
        }

        Assert.That(subject.WaitAndUnwrap(), Is.EqualTo(SENTINEL_RESULT));
    }

    [Test]
    public async Task Execute_unblocks_promise_with_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        callback().Returns(SENTINEL_RESULT);
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

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
        Assert.That(await subject.Promise, Is.EqualTo(SENTINEL_RESULT));
    }

    [Test]
    public void Cancellation_with_execute_token_declines_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            proposedCancellationToken.ThrowIfCancellationRequested();
            return SENTINEL_RESULT;
        });

        Assert.That(Execute(subject, executeMethod, proposedCancellationToken), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());
    }

    [Test]
    public void Cancellation_with_execute_token_declines_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            proposedCancellationToken.ThrowIfCancellationRequested();
            return SENTINEL_RESULT;
        });

        Assert.That(Execute(subject, executeMethod, proposedCancellationToken), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());
    }

    [Test]
    public void Arbitrary_other_cancellation_cancels_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            unknownCancellationToken.ThrowIfCancellationRequested();
            return SENTINEL_RESULT;
        });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Arbitrary_other_cancellation_cancels_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            unknownCancellationToken.ThrowIfCancellationRequested();
            return SENTINEL_RESULT;
        });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Exception_in_callback_is_being_unwrapped_by_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        callback.When(x => x()).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Exception_in_callback_is_being_unwrapped_by_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        callback.When(x => x()).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void IsCancellationRequested_returns_false([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext);

        Assert.That(((IScheduled)subject).IsCancellationRequested, Is.False);
    }

    [Test]
    public void TaskCompletionSource_runs_continuations_asynchronously()
    {
        var callback = Substitute.For<Func<int>>();
        var subject = new ScheduledNonCancellableSyncFunction<int>(callback, executionContext: null);

        var tcsField = subject.GetType().GetField("_taskCompletionSource", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var tcs = (TaskCompletionSource<int>)tcsField.GetValue(subject)!;

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
