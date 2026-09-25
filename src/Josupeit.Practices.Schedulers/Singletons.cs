// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
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
    //
    // The lookup never throws: a runtime without such a field leaves it null, and only work that
    // actually needs it fails, with PlatformNotSupportedException rather than a
    // TypeInitializationException that would poison the whole type.
    private static readonly ExecutionContext? _defaultExecutionContext = FindDefaultExecutionContext();

    public static ExecutionContext DefaultExecutionContext
        => _defaultExecutionContext
            ?? throw new PlatformNotSupportedException(
                "This runtime exposes no default ExecutionContext, so work scheduled without a captured context " +
                "(the Unsafe* families, or any call under ExecutionContext.SuppressFlow) cannot run on it.");

#if NET8_0_OR_GREATER

    // UnsafeAccessor is fully trim- and AOT-safe: the linker understands it and preserves the
    // target field, so no reflection or dynamic code is involved.
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "Default")]
    private static extern ref readonly ExecutionContext DefaultField(ExecutionContext? _);

    private static ExecutionContext? FindDefaultExecutionContext() => DefaultField(null);

#else

    // netstandard has no UnsafeAccessor, so fall back to plain reflection. .NET Core names the field
    // Default; .NET Framework keeps its pre-allocated default context in s_dummyDefaultEC. This runs
    // once at type initialization; these targets are not AOT-analyzed.
    private static ExecutionContext? FindDefaultExecutionContext()
    {
        var field = typeof(ExecutionContext).GetField("Default", BindingFlags.Static | BindingFlags.NonPublic)
            ?? typeof(ExecutionContext).GetField("s_dummyDefaultEC", BindingFlags.Static | BindingFlags.NonPublic);

        return field?.GetValue(null) as ExecutionContext;
    }

#endif
}
