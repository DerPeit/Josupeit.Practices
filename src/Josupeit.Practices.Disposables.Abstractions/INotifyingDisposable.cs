using System;
using JetBrains.Annotations;

namespace Josupeit.Practices.Disposables;

/// <summary>
/// This interface provides a mechanism for releasing resources,
/// querying whether resources have already been released and notifying
/// observers of releasing resources.
/// </summary>
/// <seealso cref="IDisposable" />
[PublicAPI]
public interface INotifyingDisposable : IDisposable
{
    /// <summary>
    /// Occurs when this instance is about to release its resources.
    /// </summary>
    event EventHandler? Disposing;

    /// <summary>
    /// Gets a value indicating whether this instance has released its resources.
    /// </summary>
    /// <value><c>true</c> if this instance has released its resources; otherwise, <c>false</c>.</value>
    bool IsDisposed { get; }
}