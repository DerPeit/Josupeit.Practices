// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(ScheduledCancellableSyncFunction<>))]
public class ScheduledCancellableSyncFunctionTests
{
    private const int SENTINEL_RESULT = 42;

    [Test]
    public void Precanceled_schedule_token_cancels_promise()
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, new CancellationToken(canceled: true), ExecutionContext.Capture());

        Assert.That(subject.IsCancellationRequested, Is.True);
        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Precanceled_schedule_token_makes_Decline_noop()
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, new CancellationToken(canceled: true), ExecutionContext.Capture());

        subject.Decline();

        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Precanceled_schedule_token_makes_Execute_noop(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, new CancellationToken(canceled: true), ExecutionContext.Capture());

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Decline_cancels_wait_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        subject.Decline();

        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Decline_cancels_promise_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        subject.Decline();

        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Execute_with_canceled_token_cancels_wait_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Execute_with_canceled_token_cancels_promise_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Execute_invokes_callback_and_returns_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        callback(Arg.Any<CancellationToken>()).Returns(SENTINEL_RESULT);
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        Assert.That(subject.WaitAndUnwrap(), Is.EqualTo(SENTINEL_RESULT));

        callback.Received(1)(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Execute_restores_execution_context([ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod)
    {
        var sentinelValue = new object();
        var sentinel1 = new AsyncLocal<object> { Value = sentinelValue };
        var sentinel2 = new AsyncLocal<object>();

        var callback = Substitute.For<Func<CancellationToken, int>>();
        var ec = ExecutionContext.Capture();

        sentinel2.Value = sentinelValue;

        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, ec);
        using var e = new ManualResetEventSlim(initialState: false);

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
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

        Assert.That(e.Wait(TimeSpan.FromSeconds(30)), Is.True);
        callback.Received(1)(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_does_not_leak_different_execution_context([ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod)
    {
        var sentinelValue = new object();
        var sentinel = new AsyncLocal<object> { Value = sentinelValue };

        var callback = Substitute.For<Func<CancellationToken, int>>();

        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext: null);

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
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

        callback.Received(1)(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_unblocks_wait_with_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        callback(Arg.Any<CancellationToken>()).Returns(SENTINEL_RESULT);
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

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
        var callback = Substitute.For<Func<CancellationToken, int>>();
        callback(Arg.Any<CancellationToken>()).Returns(SENTINEL_RESULT);
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

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
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
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
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
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
    public void Cancellation_with_schedule_token_cancels_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();

        using var scheduleCts = new CancellationTokenSource();
        var scheduleCancellationToken = scheduleCts.Token;

        var subject = new ScheduledCancellableSyncFunction<int>(callback, scheduleCancellationToken, executionContext?.CreateCopy());

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            scheduleCts.Cancel();
            scheduleCancellationToken.ThrowIfCancellationRequested();
            return SENTINEL_RESULT;
        });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);

        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Cancellation_with_schedule_token_cancels_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();

        using var scheduleCts = new CancellationTokenSource();
        var scheduleCancellationToken = scheduleCts.Token;

        var subject = new ScheduledCancellableSyncFunction<int>(callback, scheduleCancellationToken, executionContext?.CreateCopy());

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            scheduleCts.Cancel();
            scheduleCancellationToken.ThrowIfCancellationRequested();
            return SENTINEL_RESULT;
        });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);

        Assert.That(async () => await subject.Promise, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Schedule_token_canceled_before_execute_cancels_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();

        using var scheduleCts = new CancellationTokenSource();

        var subject = new ScheduledCancellableSyncFunction<int>(callback, scheduleCts.Token, executionContext?.CreateCopy());

        scheduleCts.Cancel();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);

        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Schedule_token_canceled_before_execute_cancels_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();

        using var scheduleCts = new CancellationTokenSource();

        var subject = new ScheduledCancellableSyncFunction<int>(callback, scheduleCts.Token, executionContext?.CreateCopy());

        scheduleCts.Cancel();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);

        Assert.That(async () => await subject.Promise, Throws.InstanceOf<OperationCanceledException>());

        callback.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Execute_with_both_cancellable_tokens_invokes_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        callback(Arg.Any<CancellationToken>()).Returns(SENTINEL_RESULT);

        using var scheduleCts = new CancellationTokenSource();
        using var executionCts = new CancellationTokenSource();

        var subject = new ScheduledCancellableSyncFunction<int>(callback, scheduleCts.Token, executionContext?.CreateCopy());

        Assert.That(Execute(subject, executeMethod, executionCts.Token), Throws.Nothing);
        Assert.That(subject.WaitAndUnwrap(), Is.EqualTo(SENTINEL_RESULT));

        callback.Received(1)(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Arbitrary_other_cancellation_cancels_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
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
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback(Arg.Any<CancellationToken>()).Returns(_ =>
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
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        callback.When(x => x(Arg.Any<CancellationToken>())).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);

        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Exception_in_callback_is_being_unwrapped_by_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext?.CreateCopy());

        callback.When(x => x(Arg.Any<CancellationToken>())).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);

        Assert.That(async () => await subject.Promise, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void TaskCompletionSource_runs_continuations_asynchronously()
    {
        var callback = Substitute.For<Func<CancellationToken, int>>();
        var subject = new ScheduledCancellableSyncFunction<int>(callback, CancellationToken.None, executionContext: null);

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
