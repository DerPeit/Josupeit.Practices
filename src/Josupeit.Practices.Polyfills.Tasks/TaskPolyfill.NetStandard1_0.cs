using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Polyfills.Tasks;

[DebuggerNonUserCode]
public static class TaskPolyfill
{
    public static readonly Task CompletedTask = Task.FromResult(0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> FromResult<TResult>(TResult result) => Task.FromResult(result);

    public static Task FromCanceled(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            throw new ArgumentOutOfRangeException(nameof(cancellationToken));

        return new Task(() => { }, cancellationToken);
    }

    public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            throw new ArgumentOutOfRangeException(nameof(cancellationToken));

#pragma warning disable CS8603 // Possible null reference return is a false positive, because the task won't ever have this result.
        return new Task<TResult>(() => default, cancellationToken);
#pragma warning restore CS8603
    }

    public static Task FromException(Exception ex) => FromException<int>(ex);

    public static Task<TResult> FromException<TResult>(Exception ex)
    {
        var tcs = new TaskCompletionSource<TResult>();
        tcs.TrySetException(ex);
        return tcs.Task;
    }
}
