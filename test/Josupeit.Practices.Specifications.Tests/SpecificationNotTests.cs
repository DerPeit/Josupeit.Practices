// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications.Tests;

[TestOf(typeof(Specification<>))]
public class SpecificationNotTests
{
    [Test]
    public void IsSatisfiedBy_returns_false_when_operand_is_satisfied()
    {
        var negated = !CreateSpecification(true);

        Assert.That(negated.IsSatisfiedBy(0), Is.False);
    }

    [Test]
    public void IsSatisfiedBy_returns_true_when_operand_is_not_satisfied()
    {
        var negated = !CreateSpecification(false);

        Assert.That(negated.IsSatisfiedBy(0), Is.True);
    }

    [Test]
    public async Task IsSatisfiedByAsync_returns_false_when_operand_is_satisfied()
    {
        var negated = !CreateSpecification(true);

        Assert.That(await negated.IsSatisfiedByAsync(0), Is.False);
    }

    [Test]
    public async Task IsSatisfiedByAsync_returns_true_when_operand_is_not_satisfied()
    {
        var negated = !CreateSpecification(false);

        Assert.That(await negated.IsSatisfiedByAsync(0), Is.True);
    }

    private static Specification<int> CreateSpecification(bool isSatisfied)
    {
        var specification = Substitute.ForPartsOf<Specification<int>>();

        specification.When(x => x.IsSatisfiedBy(Arg.Any<int>())).DoNotCallBase();
        specification.IsSatisfiedBy(Arg.Any<int>()).Returns(isSatisfied);

        return specification;
    }
}
