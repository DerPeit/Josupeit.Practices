using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal interface IScheduledFunction<TResult> : IScheduled
{
    Task<TResult> Promise { get; }

    TResult WaitAndUnwrap();
}