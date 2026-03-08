// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers.Channel.Tests;

[TestFixture]
[TestOf(typeof(ChannelScheduler))]
public class ChannelSchedulerLifecycleTests
{
    [Test]
    public void Constructor_validates_options()
    {
        Assert.That(() => new ChannelScheduler(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void StartAsync_with_invalid_degree_of_parallelism_throws()
    {
        var options = new ChannelSchedulerOptions { DegreeOfParallelism = 0 };
        var scheduler = new ChannelScheduler(options);

        Assert.That(async () => await scheduler.StartAsync(CancellationToken.None), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void Schedule_on_stopped_scheduler_throws()
    {
        var options = new ChannelSchedulerOptions();
        var scheduler = new ChannelScheduler(options);

        Assert.That(() => scheduler.Schedule(() => { }), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public async Task StartAsync_then_StopAsync_succeeds()
    {
        var options = new ChannelSchedulerOptions { DegreeOfParallelism = 1 };
        var scheduler = new ChannelScheduler(options);

        await scheduler.StartAsync(CancellationToken.None);
        await scheduler.StopAsync(CancellationToken.None);

        Assert.That(scheduler.State, Is.EqualTo(Services.ServiceState.Stopped));
    }
}
