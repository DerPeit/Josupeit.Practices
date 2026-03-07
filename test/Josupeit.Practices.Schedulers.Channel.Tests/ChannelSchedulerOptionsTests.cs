// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

namespace Josupeit.Practices.Schedulers.Channel.Tests;

[TestFixture]
[TestOf(typeof(ChannelSchedulerOptions))]
public class ChannelSchedulerOptionsTests
{
    [Test]
    public void Default_DegreeOfParallelism_is_ProcessorCount()
    {
        var options = new ChannelSchedulerOptions();

        Assert.That(options.DegreeOfParallelism, Is.EqualTo(Environment.ProcessorCount));
    }

    [Test]
    public void Default_GracefulStopDelay_is_three_seconds()
    {
        var options = new ChannelSchedulerOptions();

        Assert.That(options.GracefulStopDelay, Is.EqualTo(TimeSpan.FromSeconds(3)));
    }
}
