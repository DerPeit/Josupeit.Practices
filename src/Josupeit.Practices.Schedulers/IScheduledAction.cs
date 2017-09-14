using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal interface IScheduledAction : IScheduled
{
    Task Promise { get; }

    void Wait();
}