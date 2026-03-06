using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications.Tests;

[TestOf(typeof(Specification<>))]
public class SpecificationTests
{
    [Test]
    public void IsSatisfiedBy_fails_if_neither_IsSatisfiedBy_nor_IsSatisfiedByAsync_were_overridden()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.Throws<InvalidOperationException>(() => specification.IsSatisfiedBy(0));
    }

    [Test]
    public void IsSatisfiedByAsync_fails_if_neither_IsSatisfiedBy_nor_IsSatisfiedByAsync_were_overridden()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await specification.IsSatisfiedByAsync(0));
    }

    [Test]
    public void IsSatisfiedBy_calls_overridden_IsSatisfiedByAsync()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();
        var tcs = new TaskCompletionSource<bool>();

        tcs.SetResult(true);
        specification.When(x => x.IsSatisfiedByAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())).DoNotCallBase();
        specification.IsSatisfiedByAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);

        var result = specification.IsSatisfiedBy(0);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsSatisfiedByAsync_calls_overridden_IsSatisfiedBy()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        specification.When(x => x.IsSatisfiedBy(Arg.Any<int>())).DoNotCallBase();
        specification.IsSatisfiedBy(Arg.Any<int>()).Returns(true);

        var result = await specification.IsSatisfiedByAsync(0);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSatisfiedBy_unwraps_exceptions_from_IsSatisfiedByAsync()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();
        var tcs = new TaskCompletionSource<bool>();

        tcs.SetException(new TestException());
        specification.When(x => x.IsSatisfiedByAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())).DoNotCallBase();
        specification.IsSatisfiedByAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);

        Assert.Throws<TestException>(() => specification.IsSatisfiedBy(0));
    }

    [Test]
    public void IsSatisfiedByAsync_wraps_exceptions_from_IsSatisfiedBy()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        specification.When(x => x.IsSatisfiedBy(Arg.Any<int>())).Throw<TestException>();

        Task<bool> promise = null!;

        Assert.DoesNotThrow(() => promise = specification.IsSatisfiedByAsync(0));
        Assert.ThrowsAsync<TestException>(async () => await promise);
    }

    [Test]
    public void IsSatisfiedByAsync_does_not_call_IsSatisfiedBy_when_canceled_early()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        specification.When(x => x.IsSatisfiedBy(Arg.Any<int>())).DoNotCallBase();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await specification.IsSatisfiedByAsync(0, new CancellationToken(canceled: true)));
        Assert.DoesNotThrow(() => specification.DidNotReceive().IsSatisfiedBy(Arg.Any<int>()));
    }

    [Test]
    public void And_operator_throws_when_left_is_null()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.Throws<ArgumentNullException>(() => _ = (Specification<int>)null! & specification);
    }

    [Test]
    public void And_operator_throws_when_right_is_null()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.Throws<ArgumentNullException>(() => _ = specification & (Specification<int>)null!);
    }

    [Test]
    public void Or_operator_throws_when_left_is_null()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.Throws<ArgumentNullException>(() => _ = (Specification<int>)null! | specification);
    }

    [Test]
    public void Or_operator_throws_when_right_is_null()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.Throws<ArgumentNullException>(() => _ = specification | (Specification<int>)null!);
    }

    [Test]
    public void Xor_operator_throws_when_left_is_null()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.Throws<ArgumentNullException>(() => _ = (Specification<int>)null! ^ specification);
    }

    [Test]
    public void Xor_operator_throws_when_right_is_null()
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        Assert.Throws<ArgumentNullException>(() => _ = specification ^ (Specification<int>)null!);
    }

    [Test]
    public void Not_operator_throws_when_operand_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => _ = !(Specification<int>)null!);
    }
}
