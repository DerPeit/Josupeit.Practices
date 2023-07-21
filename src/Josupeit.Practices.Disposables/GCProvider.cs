using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Josupeit.Practices.Disposables.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Josupeit.Practices.Disposables;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is consistent with System.GC.")]
[DebuggerNonUserCode]
[ExcludeFromCodeCoverage]
internal abstract class GCProvider
{
    private static readonly object _syncRoot = new();

    private static GCProvider? _current;

    public static GCProvider Current
    {
        get
        {
            var current = _current;

            if (current is not null)
                return current;

            lock (_syncRoot)
                return _current ??= Default;
        }

        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            lock (_syncRoot)
                _current = value;
        }
    }

    public static GCProvider Default { get; } = new DefaultGCProvider();

    public abstract void SuppressFinalize(object obj);

    private sealed class DefaultGCProvider : GCProvider
    {
        public override void SuppressFinalize(object obj)
        {
            GC.SuppressFinalize(obj);
        }
    }
}