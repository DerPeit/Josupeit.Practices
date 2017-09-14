namespace Josupeit.Practices.Schedulers;

/// <summary>
/// Represents a synchronous enqueue callback.
/// </summary>
/// <param name="scheduled">The item to be scheduled.</param>
public delegate void SyncEnqueueCallback(IScheduled scheduled);