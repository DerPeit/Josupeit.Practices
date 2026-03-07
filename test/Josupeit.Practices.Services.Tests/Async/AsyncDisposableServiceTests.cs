// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

using System;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(AsyncDisposableService))]
public class AsyncDisposableServiceTests : ServiceTests<AsyncDisposableService>
{
    [Test]
    public void AsyncDisposableService_implements_IAsyncDisposable()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableService>();

        Assert.That(subject, Is.AssignableTo<IAsyncDisposable>());
    }
}

#endif
