using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// Represents a asynchronous enqueue callback.
/// </summary>
/// <param name="scheduled">The item to be scheduled.</param>
/// <param name="enqueueCancellationToken">The enqueue cancellation token.</param>
/// <returns>A task.</returns>
public delegate Task AsyncEnqueueCallback(IScheduled scheduled, CancellationToken enqueueCancellationToken);