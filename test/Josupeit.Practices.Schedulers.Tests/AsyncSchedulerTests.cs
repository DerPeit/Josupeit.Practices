using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncScheduler)), UsedImplicitly]
public class AsyncSchedulerTests : SchedulerTests<AsyncScheduler>;