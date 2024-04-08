#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

using Josupeit.Practices.Disposables;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(AsyncDisposableAsyncService))]
public class AsyncDisposableAsyncServiceTests : AsyncServiceTests<AsyncDisposableAsyncService>
{
    [Test]
    public void AsyncDisposableAsyncService_inherits_from_ImplicitAsyncDisposable()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableAsyncService>();

        Assert.That(subject, Is.AssignableTo<ImplicitAsyncDisposable>());
    }
}

#endif