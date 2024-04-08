using Josupeit.Practices.Disposables;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(DisposableService))]
public class DisposableServiceTests : ServiceTests<DisposableService>
{
    [Test]
    public void DisposableService_inherits_from_ImplicitDisposable()
    {
        var subject = Substitute.ForPartsOf<DisposableService>();

        Assert.That(subject, Is.AssignableTo<ImplicitDisposable>());
    }
}