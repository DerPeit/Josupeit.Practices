using System;

namespace Josupeit.Practices.Disposables;

/// <summary>
/// This interface provides a mechanism for releasing resources,
/// querying whether resources have already been released and notifying
/// observers of releasing resources.
/// </summary>
/// <seealso cref="IDisposable" />
public interface INotifyingAsyncDisposable : INotifyingDisposable, IAsyncDisposable
{
}
