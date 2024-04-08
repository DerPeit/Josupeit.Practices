using Josupeit.Practices.Disposables;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(DisposableAsyncService))]
public class DisposableAsyncServiceTests : AsyncServiceTests<DisposableAsyncService>
{
    [Test]
    public void DisposableAsyncService_inherits_from_ImplicitDisposable()
    {
        var subject = Substitute.ForPartsOf<DisposableAsyncService>();

        Assert.That(subject, Is.AssignableTo<ImplicitDisposable>());
    }
}