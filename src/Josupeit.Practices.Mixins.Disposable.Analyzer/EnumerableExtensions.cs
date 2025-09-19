using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Josupeit.Practices.Mixins.Disposable.Analyzer;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T item)
    {
        yield return item;

        foreach (var remainder in enumerable)
            yield return remainder;
    }

    public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> enumerable, SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        foreach (var item in enumerable)
        {
            yield return item;

            context.CancellationToken.ThrowIfCancellationRequested();
        }
    }
}