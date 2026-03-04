using NUnit.Framework;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncScheduler))]
public class AsyncSchedulerTests : SchedulerTests<AsyncScheduler>;