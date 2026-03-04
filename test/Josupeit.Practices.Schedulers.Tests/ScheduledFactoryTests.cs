using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(ScheduledFactory))]
public class ScheduledFactoryTests
{
    [Test]
    public void Factory_validates_delegate_argument([ValueSource(nameof(EnumerateProposedDelegates))] Delegate proposedDelegate)
    {
        var subject = new ScheduledFactory();
        var subjectType = subject.GetType();

        var createMethod = subjectType
            .GetMethods()
            .Where(mi => mi.Name == nameof(subject.Create))
            .Select(mi => mi.ContainsGenericParameters ? mi.MakeGenericMethod(typeof(object)) : mi)
            .Single(mi => mi.GetParameters()[0].ParameterType == proposedDelegate.GetType());

        var hasCancellationToken = createMethod.GetParameters().Last().ParameterType == typeof(CancellationToken);

        object?[] arguments = hasCancellationToken ? [null, null, null] : [null, null];

        Assert.That(() => createMethod.Invoke(subject, arguments), Throws.TypeOf<TargetInvocationException>().With.InnerException.TypeOf<ArgumentNullException>());
    }

    [Test, Sequential]
    public void Factory_creates_correct_types_of_schedulee(
        [ValueSource(nameof(EnumerateProposedDelegates))] Delegate proposedDelegate,
        [ValueSource(nameof(EnumerateExpectedScheduledTypes))] Type expectedScheduledType)
    {
        var subject = new ScheduledFactory();
        var subjectType = subject.GetType();

        var createMethod = subjectType
            .GetMethods()
            .Where(mi => mi.Name == nameof(subject.Create))
            .Select(mi => mi.ContainsGenericParameters ? mi.MakeGenericMethod(typeof(object)) : mi)
            .Single(mi => mi.GetParameters()[0].ParameterType == proposedDelegate.GetType());

        var hasCancellationToken = createMethod.GetParameters().Last().ParameterType == typeof(CancellationToken);

        object?[] arguments = hasCancellationToken ? [proposedDelegate, null, null] : [proposedDelegate, null];

        var schedulee = createMethod.Invoke(subject, arguments);

        Assert.That(schedulee, Is.TypeOf(expectedScheduledType));
    }

    private static IEnumerable<Delegate> EnumerateProposedDelegates()
    {
        yield return Substitute.For<Action>();
        yield return Substitute.For<Action<CancellationToken>>();
        yield return Substitute.For<Func<Task>>();
        yield return Substitute.For<Func<CancellationToken, Task>>();
        yield return Substitute.For<Func<object>>();
        yield return Substitute.For<Func<CancellationToken, object>>();
        yield return Substitute.For<Func<Task<object>>>();
        yield return Substitute.For<Func<CancellationToken, Task<object>>>();

    }

    private static IEnumerable<Type> EnumerateExpectedScheduledTypes()
    {
        yield return typeof(ScheduledNonCancellableSyncAction);
        yield return typeof(ScheduledCancellableSyncAction);
        yield return typeof(ScheduledNonCancellableAsyncAction);
        yield return typeof(ScheduledCancellableAsyncAction);
        yield return typeof(ScheduledNonCancellableSyncFunction<object>);
        yield return typeof(ScheduledCancellableSyncFunction<object>);
        yield return typeof(ScheduledNonCancellableAsyncFunction<object>);
        yield return typeof(ScheduledCancellableAsyncFunction<object>);
    }
}