namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(DisposableService))]
public class DisposableServiceTests : ServiceTests<DisposableService>
{
    [Test]
    public void DisposableService_implements_IDisposable()
    {
        var subject = Substitute.ForPartsOf<DisposableService>();

        Assert.That(subject, Is.AssignableTo<IDisposable>());
    }
}
