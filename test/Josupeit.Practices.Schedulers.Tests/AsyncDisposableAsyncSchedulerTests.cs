// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

#if NET8_0_OR_GREATER

using System;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncDisposableAsyncScheduler))]
public class AsyncDisposableAsyncSchedulerTests : SchedulerTests<AsyncDisposableAsyncScheduler>
{
    [Test]
    public void Scheduler_implements_IAsyncDisposable()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableAsyncScheduler>();

        Assert.That(subject, Is.AssignableTo<IAsyncDisposable>());
    }
}

#endif
