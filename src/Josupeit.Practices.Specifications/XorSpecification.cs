// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications;

#pragma warning disable CA1849, RS0030 // .Result is guarded by IsCompleted; the access is synchronous by construction.

[DebuggerDisplay("({_left} ^ {_right})")]
internal sealed class XorSpecification<TCandidate> : Specification<TCandidate>
{
    private readonly Specification<TCandidate> _left;
    private readonly Specification<TCandidate> _right;

    public XorSpecification(Specification<TCandidate> left, Specification<TCandidate> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(TCandidate candidate)
    {
        return _left.IsSatisfiedBy(candidate) ^ _right.IsSatisfiedBy(candidate);
    }

    public override Task<bool> IsSatisfiedByAsync(TCandidate candidate, CancellationToken cancellationToken)
    {
        var isLeftSatisfiedPromise = _left.IsSatisfiedByAsync(candidate, cancellationToken);

        if (!isLeftSatisfiedPromise.IsCompleted)
            return AwaitedLeftAndRight(this, candidate, isLeftSatisfiedPromise, cancellationToken);

        if (isLeftSatisfiedPromise.IsCanceled || isLeftSatisfiedPromise.IsFaulted)
            return isLeftSatisfiedPromise;

        var isRightSatisfiedPromise = _right.IsSatisfiedByAsync(candidate, cancellationToken);

        if (!isRightSatisfiedPromise.IsCompleted)
            return AwaitedRight(isRightSatisfiedPromise.Result, isRightSatisfiedPromise);

        if (isRightSatisfiedPromise.IsCanceled || isRightSatisfiedPromise.IsFaulted)
            return isRightSatisfiedPromise;

        if (isLeftSatisfiedPromise.Result && !isRightSatisfiedPromise.Result)
            return isLeftSatisfiedPromise;

        if (!isLeftSatisfiedPromise.Result && isRightSatisfiedPromise.Result)
            return isRightSatisfiedPromise;

        return Singletons.False;

        static async Task<bool> AwaitedLeftAndRight(XorSpecification<TCandidate> owner, TCandidate candidate, Task<bool> isLeftSatisfiedPromise, CancellationToken cancellationToken)
        {
            return await isLeftSatisfiedPromise.ConfigureAwait(false) ^ await owner._right.IsSatisfiedByAsync(candidate, cancellationToken).ConfigureAwait(false);
        }

        static async Task<bool> AwaitedRight(bool isLeftSatisfied, Task<bool> isRightSatisfiedPromise)
        {
            return isLeftSatisfied ^ await isRightSatisfiedPromise.ConfigureAwait(false);
        }
    }
}
