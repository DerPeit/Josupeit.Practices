// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications.Tests;

[TestOf(typeof(Specification<>))]
public class SpecificationXorTests
{
    [Test]
    public void IsSatisfiedBy_returns_false_when_both_are_satisfied()
    {
        var combined = CreateSpecification(true) ^ CreateSpecification(true);

        Assert.That(combined.IsSatisfiedBy(0), Is.False);
    }

    [Test]
    public void IsSatisfiedBy_returns_true_when_only_left_is_satisfied()
    {
        var combined = CreateSpecification(true) ^ CreateSpecification(false);

        Assert.That(combined.IsSatisfiedBy(0), Is.True);
    }

    [Test]
    public void IsSatisfiedBy_returns_true_when_only_right_is_satisfied()
    {
        var combined = CreateSpecification(false) ^ CreateSpecification(true);

        Assert.That(combined.IsSatisfiedBy(0), Is.True);
    }

    [Test]
    public void IsSatisfiedBy_returns_false_when_neither_is_satisfied()
    {
        var combined = CreateSpecification(false) ^ CreateSpecification(false);

        Assert.That(combined.IsSatisfiedBy(0), Is.False);
    }

    [Test]
    public async Task IsSatisfiedByAsync_returns_false_when_both_are_satisfied()
    {
        var combined = CreateSpecification(true) ^ CreateSpecification(true);

        Assert.That(await combined.IsSatisfiedByAsync(0), Is.False);
    }

    [Test]
    public async Task IsSatisfiedByAsync_returns_true_when_only_left_is_satisfied()
    {
        var combined = CreateSpecification(true) ^ CreateSpecification(false);

        Assert.That(await combined.IsSatisfiedByAsync(0), Is.True);
    }

    [Test]
    public async Task IsSatisfiedByAsync_returns_true_when_only_right_is_satisfied()
    {
        var combined = CreateSpecification(false) ^ CreateSpecification(true);

        Assert.That(await combined.IsSatisfiedByAsync(0), Is.True);
    }

    [Test]
    public async Task IsSatisfiedByAsync_returns_false_when_neither_is_satisfied()
    {
        var combined = CreateSpecification(false) ^ CreateSpecification(false);

        Assert.That(await combined.IsSatisfiedByAsync(0), Is.False);
    }

    private static Specification<int> CreateSpecification(bool isSatisfied)
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        specification.When(x => x.IsSatisfiedBy(Arg.Any<int>())).DoNotCallBase();
        specification.IsSatisfiedBy(Arg.Any<int>()).Returns(isSatisfied);

        return specification;
    }
}
