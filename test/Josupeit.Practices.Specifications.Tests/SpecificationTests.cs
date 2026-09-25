// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Diagnostics;
using System.Linq;
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

    [Test]
    public async Task IsSatisfiedByAsync_allows_concurrent_evaluation_of_a_shared_instance_through_the_bridge()
    {
        var specification = new SlowSyncSpecification();

        var results = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => specification.IsSatisfiedByAsync(1))));

        Assert.That(results, Is.All.True);
    }

    [Test]
    public void IsSatisfiedBy_allows_concurrent_evaluation_of_a_shared_instance_through_the_bridge()
    {
        var specification = new SlowAsyncSpecification();
        var results = new bool[16];

        Parallel.For(0, results.Length, i => results[i] = specification.IsSatisfiedBy(1));

        Assert.That(results, Is.All.True);
    }

    [Test]
    public async Task IsSatisfiedByAsync_does_not_accumulate_execution_context_state_across_instances()
    {
        // The recursion guard lives in an AsyncLocal per instance. A guard that leaves its entry behind
        // makes every later evaluation in the same flow copy a growing map: 20,000 instances then take
        // tens of seconds instead of milliseconds. The bound is generous to stay clear of slow CI machines.
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < 20_000; i++)
            await new PositiveSpecification().IsSatisfiedByAsync(1);

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));
    }

    private sealed class PositiveSpecification : Specification<int>
    {
        public override bool IsSatisfiedBy(int candidate) => candidate > 0;
    }

    private sealed class SlowSyncSpecification : Specification<int>
    {
        public override bool IsSatisfiedBy(int candidate)
        {
            Thread.Sleep(20);

            return candidate > 0;
        }
    }

    private sealed class SlowAsyncSpecification : Specification<int>
    {
        public override async Task<bool> IsSatisfiedByAsync(int candidate, CancellationToken cancellationToken)
        {
            await Task.Delay(20, cancellationToken);

            return candidate > 0;
        }
    }
}
