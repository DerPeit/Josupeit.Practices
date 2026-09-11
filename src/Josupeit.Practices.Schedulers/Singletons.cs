// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading;

#if NET8_0_OR_GREATER

using System.Runtime.CompilerServices;

#else

using System.Reflection;

#endif

namespace Josupeit.Practices.Schedulers;

internal static class Singletons
{
    // ExecutionContext.Default is the pristine, empty ambient context: it carries no AsyncLocal
    // values. Scheduled work with no caller-supplied context runs under it so that ambient state
    // does not leak into the callback. It is internal with no public accessor, and it must be
    // non-null because ExecutionContext.Run rejects null (unlike Capture, which returns null for a
    // clean context), so we bind the field directly.
    public static readonly ExecutionContext DefaultExecutionContext = GetDefaultExecutionContext();

#if NET8_0_OR_GREATER

    // UnsafeAccessor is fully trim- and AOT-safe: the linker understands it and preserves the
    // target field, so no reflection or dynamic code is involved.
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "Default")]
    private static extern ref readonly ExecutionContext DefaultField(ExecutionContext? _);

    private static ExecutionContext GetDefaultExecutionContext() => DefaultField(null);

#else

    // netstandard has no UnsafeAccessor, so fall back to plain reflection. This runs once at type
    // initialization; these targets are not AOT-analyzed.
    private static ExecutionContext GetDefaultExecutionContext()
        => (ExecutionContext)typeof(ExecutionContext)
            .GetField("Default", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

#endif
}
