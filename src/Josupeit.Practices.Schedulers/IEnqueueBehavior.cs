using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal interface IEnqueueBehavior
{
    void Enqueue(IScheduled scheduled, CancellationToken enqueueCancellationToken = default);

    Task EnqueueAsync(IScheduled scheduled, CancellationToken enqueueCancellationToken);
}