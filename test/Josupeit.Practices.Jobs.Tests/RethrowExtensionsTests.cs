namespace Josupeit.Practices.Jobs.Tests;

[TestOf(typeof(RethrowExtensions))]
public class RethrowExtensionsTests
{
    [Test]
    public void ThrowIfFaulted_throws_on_null()
    {
        Assert.That(() => RethrowExtensions.ThrowIfFaulted(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void ThrowIfFaulted_rethrows_when_faulted()
    {
        var job = Substitute.For<IJob>();
        var exception = new TestException();

        job.IsFaulted.Returns(true);
        job.Exception.Returns(exception);

        Assert.That(() => job.ThrowIfFaulted(), Throws.InstanceOf<TestException>());
    }

    [Test]
    public void ThrowIfFaulted_does_not_throw_when_not_faulted()
    {
        var job = Substitute.For<IJob>();

        job.IsFaulted.Returns(false);

        Assert.That(() => job.ThrowIfFaulted(), Throws.Nothing);
    }
}
