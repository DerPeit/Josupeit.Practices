using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(ScheduledNonCancellableAsyncFunction<>))]
public class ScheduledNonCancellableAsyncFunctionTests
{
    private const int SENTINEL_RESULT = 42;

    [Test]
    public void Decline_cancels_wait_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        subject.Decline();

        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Decline_cancels_promise_without_invoking_callback([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        subject.Decline();

        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_with_cancelled_token_cancels_wait_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_with_cancelled_token_cancels_promise_without_invoking_callback(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        Assert.That(Execute(subject, executeMethod, new CancellationToken(canceled: true)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());

        callback.DidNotReceive()();
    }

    [Test]
    public void Execute_invokes_callback_and_returns_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        callback().Returns(Task.FromResult(SENTINEL_RESULT));
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

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

        var callback = Substitute.For<Func<Task<int>>>();
        var ec = ExecutionContext.Capture();

        sentinel2.Value = sentinelValue;

        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, ec);
        using var e = new ManualResetEventSlim(initialState: false);

        callback().Returns(_ =>
        {
            Assert.That(sentinel1.Value, Is.SameAs(sentinelValue));
            Assert.That(sentinel2.Value, Is.Null);
            return Task.FromResult(SENTINEL_RESULT);
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
                    case TestDelegate td:
                        td();
                        e.Set();
                        break;

                    case AsyncTestDelegate atd:
                        atd().ContinueWith(_ => e.Set(), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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

        var callback = Substitute.For<Func<Task<int>>>();

        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext: null);

        callback().Returns(_ =>
        {
            Assert.That(sentinel.Value, Is.Null);
            return Task.FromResult(SENTINEL_RESULT);
        });

        switch (Execute(subject, executeMethod, CancellationToken.None))
        {
            case TestDelegate td:
                td();
                break;

            case AsyncTestDelegate atd:
                await atd();
                break;
        }

        Assert.That(sentinel.Value, Is.SameAs(sentinelValue));

        _ = callback.Received(1)();
    }

    [Test, CancelAfter(500)]
    public async Task Execute_unblocks_wait_with_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        callback().Returns(Task.FromResult(SENTINEL_RESULT));
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        switch (Execute(subject, executeMethod, CancellationToken.None))
        {
            case TestDelegate td:
                td();
                break;

            case AsyncTestDelegate atd:
                await Task.WhenAny(atd(), Task.Delay(Timeout.Infinite, cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
                break;
        }

        Assert.That(subject.WaitAndUnwrap(), Is.EqualTo(SENTINEL_RESULT));
    }

    [Test, CancelAfter(500)]
    public async Task Execute_unblocks_promise_with_result(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        callback().Returns(Task.FromResult(SENTINEL_RESULT));
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        switch (Execute(subject, executeMethod, CancellationToken.None))
        {
            case TestDelegate td:
                td();
                break;

            case AsyncTestDelegate atd:
                await Task.WhenAny(atd(), Task.Delay(Timeout.Infinite, cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
                break;
        }

        await Task.WhenAny(subject.Promise, Task.Delay(Timeout.Infinite, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(await subject.Promise, Is.EqualTo(SENTINEL_RESULT));
    }

    [Test, CancelAfter(500)]
    public void Cancellation_with_execute_token_declines_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            return Task.FromCanceled<int>(proposedCancellationToken);
        });

        Assert.That(Execute(subject, executeMethod, proposedCancellationToken), Throws.InstanceOf<OperationCanceledException>());
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());
    }

    [Test, CancelAfter(500)]
    public void Cancellation_with_execute_token_declines_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var proposedCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            return Task.FromCanceled<int>(proposedCancellationToken);
        });

        Assert.That(Execute(subject, executeMethod, proposedCancellationToken), Throws.InstanceOf<OperationCanceledException>());
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(async () => await subject.Promise, Throws.TypeOf<ExecutionDeclinedException>());
    }

    [Test, CancelAfter(500)]
    public void Arbitrary_other_cancellation_cancels_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            return Task.FromCanceled<int>(unknownCancellationToken);
        });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test, CancelAfter(500)]
    public void Arbitrary_other_cancellation_cancels_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        using var cancellationTokenSource = new CancellationTokenSource();
        var unknownCancellationToken = cancellationTokenSource.Token;

        callback().Returns(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cancellationTokenSource.Cancel();
            return Task.FromCanceled<int>(unknownCancellationToken);
        });

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test, CancelAfter(500)]
    public void Exception_in_callback_is_being_unwrapped_by_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        callback().Returns(Task.FromException<int>(new TestException()));

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<TestException>());
    }

    [Test, CancelAfter(500)]
    public void Exception_in_callback_is_being_unwrapped_by_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        callback().Returns(Task.FromException<int>(new TestException()));

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<TestException>());
    }

    [Test, CancelAfter(500)]
    public void Synchronous_exception_in_callback_is_being_unwrapped_by_wait(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        callback.When(x => x()).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<TestException>());
    }

    [Test, CancelAfter(500)]
    public void Synchronous_exception_in_callback_is_being_unwrapped_by_promise(
        [ValueSource(nameof(ReturnExecuteThenExecuteAsync))] MethodInfo executeMethod,
        [ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext,
        CancellationToken cancellationToken = default)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        callback.When(x => x()).Throw<TestException>();

        Assert.That(Execute(subject, executeMethod, CancellationToken.None), Throws.Nothing);
        cancellationToken.ThrowIfCancellationRequested();
        Assert.That(async () => await subject.Promise, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void IsCancellationRequested_returns_false([ValueSource(nameof(ReturnCapturedContextThenNull))] ExecutionContext? executionContext)
    {
        var callback = Substitute.For<Func<Task<int>>>();
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext);

        Assert.That(((IScheduled)subject).IsCancellationRequested, Is.False);
    }

    [Test, CancelAfter(500)]
    public async Task ExecuteAsync_with_incomplete_task_succeeds()
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var callback = Substitute.For<Func<Task<int>>>();
        callback().Returns(tcs.Task);
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext: null);

        var executeTask = subject.ExecuteAsync(CancellationToken.None);

        Assert.That(executeTask.IsCompleted, Is.False);

        tcs.SetResult(SENTINEL_RESULT);
        await executeTask;

        Assert.That(subject.WaitAndUnwrap(), Is.EqualTo(SENTINEL_RESULT));
        Assert.That(await subject.Promise, Is.EqualTo(SENTINEL_RESULT));
    }

    [Test, CancelAfter(500)]
    public async Task ExecuteAsync_with_incomplete_cancelled_task_and_execute_token_declines()
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var callback = Substitute.For<Func<Task<int>>>();
        callback().Returns(tcs.Task);
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext: null);

        using var cts = new CancellationTokenSource();

        var executeTask = subject.ExecuteAsync(cts.Token);
        cts.Cancel();
        tcs.SetCanceled();

        Assert.That(async () => await executeTask, Throws.InstanceOf<OperationCanceledException>());
        Assert.That(() => subject.WaitAndUnwrap(), Throws.TypeOf<ExecutionDeclinedException>());
    }

    [Test, CancelAfter(500)]
    public async Task ExecuteAsync_with_incomplete_cancelled_task_cancels()
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var callback = Substitute.For<Func<Task<int>>>();
        callback().Returns(tcs.Task);
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext: null);

        var executeTask = subject.ExecuteAsync(CancellationToken.None);
        tcs.SetCanceled();
        await executeTask;

        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test, CancelAfter(500)]
    public async Task ExecuteAsync_with_incomplete_faulted_task_unwraps_exception()
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var callback = Substitute.For<Func<Task<int>>>();
        callback().Returns(tcs.Task);
        var subject = new ScheduledNonCancellableAsyncFunction<int>(callback, executionContext: null);

        var executeTask = subject.ExecuteAsync(CancellationToken.None);
        tcs.SetException(new TestException());
        await executeTask;

        Assert.That(() => subject.WaitAndUnwrap(), Throws.InstanceOf<TestException>());
    }

    private static Delegate Execute(IScheduled subject, MethodInfo mi, CancellationToken cancellationToken)
    {
        var call = Expression.Call(Expression.Constant(subject), mi, Expression.Constant(cancellationToken));

        if (mi.Name == nameof(IScheduled.Execute))
        {
            return Expression.Lambda<TestDelegate>(call).Compile();
        }

        return Expression.Lambda<AsyncTestDelegate>(call).Compile();
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
