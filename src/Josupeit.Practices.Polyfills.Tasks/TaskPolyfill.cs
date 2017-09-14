using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Polyfills.Tasks;

[DebuggerNonUserCode]
public static class TaskPolyfill
{
    public static Task CompletedTask => Task.CompletedTask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> FromResult<TResult>(TResult result) => Task.FromResult(result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task FromCanceled(CancellationToken cancellationToken) => Task.FromCanceled(cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) => Task.FromCanceled<TResult>(cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task FromException(Exception ex) => Task.FromException(ex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> FromException<TResult>(Exception ex) => Task.FromException<TResult>(ex);
}
