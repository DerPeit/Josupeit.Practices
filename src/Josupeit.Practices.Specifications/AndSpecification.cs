using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications;

[DebuggerDisplay("({_left} && {_right})")]
internal sealed class AndSpecification<TCandidate> : Specification<TCandidate>
{
    private readonly Specification<TCandidate> _left;
    private readonly Specification<TCandidate> _right;

    public AndSpecification(Specification<TCandidate> left, Specification<TCandidate> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(TCandidate candidate)
    {
        return _left.IsSatisfiedBy(candidate) && _right.IsSatisfiedBy(candidate);
    }

    public override Task<bool> IsSatisfiedByAsync(TCandidate candidate, CancellationToken cancellationToken)
    {
        var isLeftSatisfiedPromise = _left.IsSatisfiedByAsync(candidate, cancellationToken);

        if (!isLeftSatisfiedPromise.IsCompleted)
            return Awaited(this, candidate, isLeftSatisfiedPromise, cancellationToken);

        if (isLeftSatisfiedPromise.IsCanceled || isLeftSatisfiedPromise.IsFaulted || !isLeftSatisfiedPromise.Result)
            return isLeftSatisfiedPromise;

        return _right.IsSatisfiedByAsync(candidate, cancellationToken);

        static async Task<bool> Awaited(AndSpecification<TCandidate> owner, TCandidate candidate, Task<bool> isLeftSatisfiedPromise, CancellationToken cancellationToken)
        {
            return await isLeftSatisfiedPromise.ConfigureAwait(false) && await owner._right.IsSatisfiedByAsync(candidate, cancellationToken).ConfigureAwait(false);
        }
    }
}