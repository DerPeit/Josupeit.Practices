using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(Scheduler))]
public class SchedulerTests : SchedulerTests<Scheduler>;

public abstract class SchedulerTests<TScheduler> where TScheduler : class, IScheduler
{
    private IScheduledFactory _scheduledFactoryMock = null!;
    private IEnqueueBehavior _enqueueBehaviorMock = null!;

    private TScheduler _subject = null!;

    [SetUp]
    public void Setup()
    {
        _scheduledFactoryMock = Substitute.For<IScheduledFactory>();
        _enqueueBehaviorMock = Substitute.For<IEnqueueBehavior>();

        _subject = Substitute.ForPartsOf<TScheduler>(_scheduledFactoryMock, _enqueueBehaviorMock);
    }

    [Test]
    public void All_public_methods_validate_reference_arguments([ValueSource(nameof(EnumerateMethods))] MethodInfo method)
    {
        if (method.ContainsGenericParameters)
            method = method.MakeGenericMethod(typeof(object));

        var args = new List<object?>(capacity: 7);

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.ParameterType.IsClass)
                args.Add(null);
            else
                args.Add(Activator.CreateInstance(parameter.ParameterType));
        }

        Assert.That(
            async () =>
            {
                var result = method.Invoke(_subject, args.ToArray());

                if (result is Task promise)
                    await promise.ConfigureAwait(false);
            },
            Throws.TypeOf<TargetInvocationException>().With.InnerException.TypeOf<ArgumentNullException>().Or.TypeOf<ArgumentNullException>());
    }

    [Test]
    public async Task All_schedule_methods_delegate_scheduled_creation_to_factory([ValueSource(nameof(EnumerateMethods))] MethodInfo method)
    {
        if (method.ContainsGenericParameters)
            method = method.MakeGenericMethod(typeof(object));

        var args = new List<object?>(capacity: 7);

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken))
                args.Add(new CancellationTokenSource().Token);
            else
                args.Add(Substitute.For([parameter.ParameterType], Array.Empty<object>()));
        }

        if (method.Invoke(_subject, args.ToArray()) is Task promise)
            await promise;

        var receivedCalls = _scheduledFactoryMock.ReceivedCalls().ToList();

        Assert.That(receivedCalls.Count, Is.EqualTo(1));
        Assert.That(receivedCalls[0].GetMethodInfo().Name, Is.EqualTo(nameof(IScheduledFactory.Create)));
        
        var receivedArguments = receivedCalls[0].GetArguments();
        
        Assert.That(receivedArguments[0], Is.SameAs(args[0]));

        var lastReceivedArgument = receivedArguments.Last();

        if (lastReceivedArgument is CancellationToken)
        {
            var expectedCancellationToken = args.Last() is CancellationToken ct ? ct : CancellationToken.None;
            
            Assert.That(lastReceivedArgument, Is.EqualTo(expectedCancellationToken));
        }
    }

    [Test]
    public async Task All_safe_schedule_methods_should_capture_execution_context([ValueSource(nameof(EnumerateSafeMethods))] MethodInfo method)
    {
        if (method.ContainsGenericParameters)
            method = method.MakeGenericMethod(typeof(object));

        var args = new List<object?>(capacity: 7);

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken))
                args.Add(new CancellationTokenSource().Token);
            else
                args.Add(Substitute.For([parameter.ParameterType], Array.Empty<object>()));
        }

        var expectedTestHelperValue = new object();
        var testHelper = new AsyncLocal<object>
        {
            Value = expectedTestHelperValue
        };

        if (method.Invoke(_subject, args.ToArray()) is Task promise)
            await promise;

        var createCall = _scheduledFactoryMock.ReceivedCalls().Single();
        var indexOfExecutionContextArgument = Array.FindIndex(createCall.GetParameterInfos(), pi => pi.ParameterType == typeof(ExecutionContext));
        var proposedExecutionContext = createCall.GetArguments()[indexOfExecutionContextArgument] as ExecutionContext;

        Assert.That(proposedExecutionContext, Is.Not.Null);

        using var semaphore = new SemaphoreSlim(initialCount: 0);

        ThreadPool.UnsafeQueueUserWorkItem(
            static state =>
            {
                var proposedExecutionContext = (ExecutionContext)((object[])state!)[0];
                var testHelper = (AsyncLocal<object>)((object[])state)[1];
                var semaphore = (SemaphoreSlim)((object[])state)[2];
                var expectedTestHelperValue = ((object[])state)[3];

                Assert.That(testHelper.Value, Is.Null); // to ensure that the test did not accidentally restore the execution context

                ExecutionContext.Run(
                    proposedExecutionContext,
                    static state =>
                    {
                        var testHelper = (AsyncLocal<object>)((object[])state!)[0];
                        var expectedTestHelperValue = ((object[])state)[1];

                        Assert.That(testHelper.Value, Is.SameAs(expectedTestHelperValue));
                    },
                    state: new[] { testHelper, expectedTestHelperValue });

                semaphore.Release();
            },
            state: new[] { proposedExecutionContext, testHelper, semaphore, expectedTestHelperValue });

        await semaphore.WaitAsync();
    }

    [Test]
    public async Task All_unsafe_schedule_methods_must_not_capture_execution_context([ValueSource(nameof(EnumerateUnsafeMethods))] MethodInfo method)
    {
        if (method.ContainsGenericParameters)
            method = method.MakeGenericMethod(typeof(object));

        var args = new List<object?>(capacity: 7);

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken))
                args.Add(new CancellationTokenSource().Token);
            else
                args.Add(Substitute.For([parameter.ParameterType], Array.Empty<object>()));
        }

        if (method.Invoke(_subject, args.ToArray()) is Task promise)
            await promise;

        var createCall = _scheduledFactoryMock.ReceivedCalls().Single();
        var indexOfExecutionContextArgument = Array.FindIndex(createCall.GetParameterInfos(), pi => pi.ParameterType == typeof(ExecutionContext));
        var proposedExecutionContext = createCall.GetArguments()[indexOfExecutionContextArgument];

        Assert.That(proposedExecutionContext, Is.Null);
    }

    [Test]
    public void All_synchronous_void_schedule_methods_must_wait_for_the_schedulee([ValueSource(nameof(EnumerateSynchronousVoidMethods))] MethodInfo method)
    {
        var createScheduledMethod = typeof(IScheduledFactory).GetMethod(
                                        nameof(IScheduledFactory.Create),
                                        [
                                            method.GetParameters()[0].ParameterType,
                                            typeof(ExecutionContext),
                                            typeof(CancellationToken)
                                        ])
                                    ?? typeof(IScheduledFactory).GetMethod(
                                        nameof(IScheduledFactory.Create),
                                        [
                                            method.GetParameters()[0].ParameterType,
                                            typeof(ExecutionContext)
                                        ]);

        Assert.That(createScheduledMethod, Is.Not.Null);

        var scheduledActionMock = Substitute.For<IScheduledAction>();

        createScheduledMethod!
            .Invoke(_scheduledFactoryMock, parameters: new object[createScheduledMethod.GetParameters().Length])
            .ReturnsForAnyArgs(scheduledActionMock);

        var methodArgs = method
            .GetParameters()
            .Select(p =>
            {
                if (p.ParameterType == typeof(CancellationToken))
                    return default(CancellationToken);

                return Substitute.For([p.ParameterType], constructorArguments: Array.Empty<object>());
            })
            .ToArray();

        method.Invoke(_subject, methodArgs);

        scheduledActionMock.Received().Wait();
    }

    [Test]
    public void All_synchronous_non_void_schedule_methods_must_wait_for_the_schedulee_and_unwrap_the_result([ValueSource(nameof(EnumerateSynchronousNonVoidMethods))] MethodInfo method)
    {
        method = method.MakeGenericMethod(typeof(object));

        var createScheduledMethod = typeof(IScheduledFactory)
            .GetMethods()
            .Where(mi => mi.ContainsGenericParameters)
            .Select(mi => mi.MakeGenericMethod(typeof(object)))
            .Single(mi => mi.GetParameters()[0].ParameterType == method.GetParameters()[0].ParameterType);

        var scheduledFunctionMock = Substitute.For<IScheduledFunction<object>>();

        createScheduledMethod!
            .Invoke(_scheduledFactoryMock, parameters: new object[createScheduledMethod.GetParameters().Length])
            .ReturnsForAnyArgs(scheduledFunctionMock);

        var expectedResult = new object();

        scheduledFunctionMock.WaitAndUnwrap().Returns(expectedResult);

        var methodArgs = method
            .GetParameters()
            .Select(p =>
            {
                if (p.ParameterType == typeof(CancellationToken))
                    return default(CancellationToken);

                return Substitute.For([p.ParameterType], constructorArguments: Array.Empty<object>());
            })
            .ToArray();

        var observedResult = method.Invoke(_subject, methodArgs);

        scheduledFunctionMock.Received().WaitAndUnwrap();

        Assert.That(observedResult, Is.SameAs(expectedResult));
    }

    [Test]
    public async Task All_asynchronous_void_schedule_methods_must_wait_for_the_schedulee([ValueSource(nameof(EnumerateAsynchronousVoidMethods))] MethodInfo method)
    {
        var createScheduledMethod = typeof(IScheduledFactory).GetMethod(
                                        nameof(IScheduledFactory.Create),
                                        [
                                            method.GetParameters()[0].ParameterType,
                                            typeof(ExecutionContext),
                                            typeof(CancellationToken)
                                        ])
                                    ?? typeof(IScheduledFactory).GetMethod(
                                        nameof(IScheduledFactory.Create),
                                        [
                                            method.GetParameters()[0].ParameterType,
                                            typeof(ExecutionContext)
                                        ]);

        Assert.That(createScheduledMethod, Is.Not.Null);

        var scheduledActionMock = Substitute.For<IScheduledAction>();

        createScheduledMethod!
            .Invoke(_scheduledFactoryMock, parameters: new object[createScheduledMethod.GetParameters().Length])
            .ReturnsForAnyArgs(scheduledActionMock);

        var promiseSource = new TaskCompletionSource<object>();

        scheduledActionMock.Promise.Returns(promiseSource.Task);

        var methodArgs = method
            .GetParameters()
            .Select(p =>
            {
                if (p.ParameterType == typeof(CancellationToken))
                    return default(CancellationToken);

                return Substitute.For([p.ParameterType], constructorArguments: Array.Empty<object>());
            })
            .ToArray();

        var observedPromise = (Task)method.Invoke(_subject, methodArgs)!;

        _ = scheduledActionMock.Received().Promise;

        const int never = -1;
        using var timer = new Timer(_ => promiseSource.SetCanceled(), state: null, dueTime: 100, period: never);

        try
        {
            await observedPromise;

            Assert.Fail("Promise should have been canceled.");
        }
        catch (TaskCanceledException)
        {
            // Test succeeds
        }
    }

    [Test]
    public async Task All_asynchronous_non_void_schedule_methods_must_wait_for_the_schedulee_and_unwrap_the_result([ValueSource(nameof(EnumerateAsynchronousNonVoidMethods))] MethodInfo method)
    {
        method = method.MakeGenericMethod(typeof(object));

        var createScheduledMethod = typeof(IScheduledFactory)
            .GetMethods()
            .Where(mi => mi.ContainsGenericParameters)
            .Select(mi => mi.MakeGenericMethod(typeof(object)))
            .Single(mi => mi.GetParameters()[0].ParameterType == method.GetParameters()[0].ParameterType);

        var scheduledFunctionMock = Substitute.For<IScheduledFunction<object>>();

        createScheduledMethod!
            .Invoke(_scheduledFactoryMock, parameters: new object[createScheduledMethod.GetParameters().Length])
            .ReturnsForAnyArgs(scheduledFunctionMock);

        var expectedResult = new object();
        var promiseSource = new TaskCompletionSource<object>();

        scheduledFunctionMock.Promise.Returns(promiseSource.Task);

        var methodArgs = method
            .GetParameters()
            .Select(p =>
            {
                if (p.ParameterType == typeof(CancellationToken))
                    return default(CancellationToken);

                return Substitute.For([p.ParameterType], constructorArguments: Array.Empty<object>());
            })
            .ToArray();

        var observedPromise = (Task<object>)method.Invoke(_subject, methodArgs)!;

        _ = scheduledFunctionMock.Received().Promise;

        const int never = -1;
        using var timer = new Timer(_ => promiseSource.SetResult(expectedResult), state: null, dueTime: 100, period: never);

        var observedResult = await observedPromise;

        Assert.That(observedResult, Is.SameAs(expectedResult));
    }

    [Test]
    public async Task All_schedule_methods_enqueue_schedulee([ValueSource(nameof(EnumerateMethods))] MethodInfo method)
    {
        if (method.IsGenericMethod)
            method = method.MakeGenericMethod(typeof(object));

        var arguments = method
            .GetParameters()
            .Select(p => p.ParameterType.IsClass ? Substitute.For([p.ParameterType], Array.Empty<object>()) : Activator.CreateInstance(p.ParameterType))
            .ToArray();

        if (method.Invoke(_subject, arguments) is Task promise)
            await promise;

        var createScheduleeCall = _scheduledFactoryMock.ReceivedCalls().Single();
        var schedulee = createScheduleeCall.GetMethodInfo().Invoke(createScheduleeCall.Target(), createScheduleeCall.GetArguments());

        var enqueueScheduleeCall = _enqueueBehaviorMock.ReceivedCalls().Single();

        Assert.That(enqueueScheduleeCall.GetMethodInfo().Name is "Enqueue" or "EnqueueAsync");
        Assert.That(enqueueScheduleeCall.GetArguments()[0], Is.SameAs(schedulee));
    }

    private static IEnumerable<MethodInfo> EnumerateMethods()
    {
        return typeof(TScheduler)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(mi => mi.Name.StartsWith("Schedule", StringComparison.Ordinal) || mi.Name.StartsWith("UnsafeSchedule", StringComparison.Ordinal));
    }

    private static IEnumerable<MethodInfo> EnumerateSafeMethods()
    {
        return EnumerateMethods().Where(mi => !mi.Name.StartsWith("Unsafe", StringComparison.Ordinal));
    }

    private static IEnumerable<MethodInfo> EnumerateUnsafeMethods()
    {
        return EnumerateMethods().Where(mi => mi.Name.StartsWith("Unsafe", StringComparison.Ordinal));
    }

    private static IEnumerable<MethodInfo> EnumerateSynchronousVoidMethods()
    {
        return EnumerateMethods().Where(mi => mi.ReturnType == typeof(void));
    }

    private static IEnumerable<MethodInfo> EnumerateSynchronousNonVoidMethods()
    {
        return EnumerateMethods().Where(mi => mi.IsGenericMethod && mi.ReturnType == mi.GetGenericArguments()[0]);
    }

    private static IEnumerable<MethodInfo> EnumerateAsynchronousVoidMethods()
    {
        return EnumerateMethods().Where(mi => mi.ReturnType == typeof(Task));
    }

    private static IEnumerable<MethodInfo> EnumerateAsynchronousNonVoidMethods()
    {
        return EnumerateMethods().Where(mi => mi.IsGenericMethod && mi.ReturnType == typeof(Task<>).MakeGenericType(mi.GetGenericArguments()[0]));
    }
}