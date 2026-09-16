// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications;

#pragma warning disable CA1849, RS0030 // .Result is guarded by IsCompleted; the access is synchronous by construction.

[DebuggerDisplay("!{_operand}")]
internal sealed class NotSpecification<TCandidate> : Specification<TCandidate>
{
    private readonly Specification<TCandidate> _operand;

    public NotSpecification(Specification<TCandidate> operand)
    {
        _operand = operand;
    }

    public override bool IsSatisfiedBy(TCandidate candidate)
    {
        return !_operand.IsSatisfiedBy(candidate);
    }

    public override Task<bool> IsSatisfiedByAsync(TCandidate candidate, CancellationToken cancellationToken)
    {
        var isOperandSatisfiedPromise = _operand.IsSatisfiedByAsync(candidate, cancellationToken);

        if (!isOperandSatisfiedPromise.IsCompleted)
            return Awaited(isOperandSatisfiedPromise);

        if (isOperandSatisfiedPromise.IsCanceled || isOperandSatisfiedPromise.IsFaulted)
            return isOperandSatisfiedPromise;

        if (isOperandSatisfiedPromise.Result)
            return Singletons.False;

        return Singletons.True;

        static async Task<bool> Awaited(Task<bool> isOperandSatisfiedPromise)
        {
            return !await isOperandSatisfiedPromise.ConfigureAwait(false);
        }
    }
}
