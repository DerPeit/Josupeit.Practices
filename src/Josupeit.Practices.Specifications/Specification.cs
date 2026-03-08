// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Specifications;

/// <summary>
/// Represents a specification that can be satisfied or not satisfied by a candidate.
/// </summary>
/// <typeparam name="TCandidate">The type of the candidate.</typeparam>
[PublicAPI]
public abstract class Specification<TCandidate>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private volatile bool _isRecursing;

    /// <summary>
    /// Returns <c>false</c>; enables the use of a specification in a short-circuit boolean <c>&amp;&amp;</c> or <c>||</c> expression.
    /// </summary>
    /// <param name="_">The specification (unused).</param>
    /// <returns><c>false</c>.</returns>
    public static bool operator true(Specification<TCandidate> _) => false;

    /// <summary>
    /// Returns <c>false</c>; enables the use of a specification in a short-circuit boolean <c>&amp;&amp;</c> or <c>||</c> expression.
    /// </summary>
    /// <param name="_">The specification (unused).</param>
    /// <returns><c>false</c>.</returns>
    public static bool operator false(Specification<TCandidate> _) => false;

    /// <summary>
    /// Combines two specifications with a logical AND.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A specification that is satisfied only when both <paramref name="left"/> and <paramref name="right"/> are satisfied.</returns>
    public static Specification<TCandidate> operator &(Specification<TCandidate> left, Specification<TCandidate> right)
        => new AndSpecification<TCandidate>(
            left ?? throw new ArgumentNullException(nameof(left)),
            right ?? throw new ArgumentNullException(nameof(right)));

    /// <summary>
    /// Combines two specifications with a logical OR.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A specification that is satisfied when either <paramref name="left"/> or <paramref name="right"/> is satisfied.</returns>
    public static Specification<TCandidate> operator |(Specification<TCandidate> left, Specification<TCandidate> right)
        => new OrSpecification<TCandidate>(
            left ?? throw new ArgumentNullException(nameof(left)),
            right ?? throw new ArgumentNullException(nameof(right)));

    /// <summary>
    /// Combines two specifications with a logical exclusive OR (XOR).
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A specification that is satisfied when exactly one of <paramref name="left"/> or <paramref name="right"/> is satisfied.</returns>
    public static Specification<TCandidate> operator ^(Specification<TCandidate> left, Specification<TCandidate> right)
        => new XorSpecification<TCandidate>(
            left ?? throw new ArgumentNullException(nameof(left)),
            right ?? throw new ArgumentNullException(nameof(right)));

    /// <summary>
    /// Negates a specification.
    /// </summary>
    /// <param name="operand">The specification to negate.</param>
    /// <returns>A specification that is satisfied when <paramref name="operand"/> is not satisfied.</returns>
    public static Specification<TCandidate> operator !(Specification<TCandidate> operand)
        => new NotSpecification<TCandidate>(
            operand ?? throw new ArgumentNullException(nameof(operand)));

    /// <summary>
    /// Determines whether this instance is satisfied by the supplied candidate.
    /// </summary>
    /// <param name="candidate">The candidate.</param>
    /// <returns><c>true</c> if this instance is satisfied by the candidate; otherwise, <c>false</c>.</returns>
    public virtual bool IsSatisfiedBy(TCandidate candidate)
    {
        using (CreateRecursionScope())
        {
            return IsSatisfiedByAsync(candidate, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Determines asynchronously whether this instance is satisfied by the supplied candidate.
    /// </summary>
    /// <param name="candidate">The candidate.</param>
    /// <returns><c>true</c> if this instance is satisfied by the candidate; otherwise, <c>false</c>.</returns>
    public Task<bool> IsSatisfiedByAsync(TCandidate candidate) => IsSatisfiedByAsync(candidate, CancellationToken.None);

    /// <summary>
    /// Determines asynchronously whether this instance is satisfied by the supplied candidate.
    /// </summary>
    /// <param name="candidate">The candidate.</param>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns><c>true</c> if this instance is satisfied by the candidate; otherwise, <c>false</c>.</returns>
    public virtual Task<bool> IsSatisfiedByAsync(TCandidate candidate, CancellationToken cancellationToken)
    {
        using (CreateRecursionScope())
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<bool>(cancellationToken);

            try
            {
                var isSatisfied = IsSatisfiedBy(candidate);

                return Task.FromResult(isSatisfied);
            }
            catch (Exception ex)
            {
                return Task.FromException<bool>(ex);
            }
        }
    }
        
    private IDisposable CreateRecursionScope()
    {
        var isRecursing = _isRecursing;
        _isRecursing = true;

        if (isRecursing)
            throw new InvalidOperationException(
                $"Neither the synchronous method {nameof(IsSatisfiedBy)}, nor the asynchronous overload " +
                $"{nameof(IsSatisfiedByAsync)} has been overridden or either base.{nameof(IsSatisfiedBy)} " +
                $"or base.{nameof(IsSatisfiedByAsync)} has been called from user-code which is prohibited. " +
                $"Please ensure to override at least one of these methods and remove base calls and try again.");

        return new RecursionScope(this);
    }

    private sealed class RecursionScope : IDisposable
    {
        private readonly Specification<TCandidate> _owner;

        public RecursionScope(Specification<TCandidate> owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner._isRecursing = false;
        }
    }
}
