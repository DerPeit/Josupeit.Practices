using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications;

/// <summary>
/// Represents a specification that can be satisfied or not satisfied by a candidate.
/// </summary>
/// <typeparam name="TCandidate">The type of the candidate.</typeparam>
public abstract class Specification<TCandidate>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private volatile bool _isRecursing;

    public static bool operator true(Specification<TCandidate> _) => false;
    
    public static bool operator false(Specification<TCandidate> _) => false;

    public static Specification<TCandidate> operator &(Specification<TCandidate> left, Specification<TCandidate> right)
        => new AndSpecification<TCandidate>(
            left ?? throw new ArgumentNullException(nameof(left)),
            right ?? throw new ArgumentNullException(nameof(right)));

    public static Specification<TCandidate> operator |(Specification<TCandidate> left, Specification<TCandidate> right)
        => new OrSpecification<TCandidate>(
            left ?? throw new ArgumentNullException(nameof(left)),
            right ?? throw new ArgumentNullException(nameof(right)));

    public static Specification<TCandidate> operator ^(Specification<TCandidate> left, Specification<TCandidate> right)
        => new XorSpecification<TCandidate>(
            left ?? throw new ArgumentNullException(nameof(left)),
            right ?? throw new ArgumentNullException(nameof(right)));

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