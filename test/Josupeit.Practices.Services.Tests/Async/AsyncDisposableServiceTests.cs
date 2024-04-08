#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

using Josupeit.Practices.Disposables;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(AsyncDisposableService))]
public class AsyncDisposableServiceTests : ServiceTests<AsyncDisposableService>
{
    [Test]
    public void AsyncDisposableAsyncService_inherits_from_ImplicitAsyncDisposable()
    {
        var subject = Substitute.ForPartsOf<AsyncDisposableService>();

        Assert.That(subject, Is.AssignableTo<ImplicitAsyncDisposable>());
    }
}

#endif