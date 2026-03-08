// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(DisposableAsyncService))]
public class DisposableAsyncServiceTests : AsyncServiceTests<DisposableAsyncService>
{
    [Test]
    public void DisposableAsyncService_implements_IDisposable()
    {
        var subject = Substitute.ForPartsOf<DisposableAsyncService>();

        Assert.That(subject, Is.AssignableTo<IDisposable>());
    }
}
