using System;
using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// This class represents configuration options for the <see cref="ChannelScheduler"/>.
/// </summary>
[PublicAPI]
public class ChannelSchedulerOptions
{
    /// <summary>
    /// The default degree of parallelism, which is currently the number of logical processors.
    /// </summary>
    public static readonly int DefaultDegreeOfParallelism = Environment.ProcessorCount;

    /// <summary>
    /// The default delay to wait for pending work gracefully when the scheduler is stopped,
    /// which is currently 3 seconds.
    /// </summary>
    public static readonly TimeSpan DefaultGracefulStopDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Gets or sets the degree of parallelism.
    /// </summary>
    /// <remarks>
    /// The <see cref="ChannelScheduler"/> will process <c>n</c> work items in parallel
    /// where <c>n</c> is the value configured here.
    /// </remarks>
    /// <value>The degree of parallelism.</value>
    public int DegreeOfParallelism { get; set; } = DefaultDegreeOfParallelism;

    /// <summary>
    /// Gets or sets the delay to wait for pending work gracefully when the scheduler is stopped.
    /// </summary>
    /// <remarks>
    /// When this timeout has elapsed the work will be canceled.<para />
    /// Set this value to zero (<c>0</c>) to cancel running work immediately when stopped.<para />
    /// Set this value to any negative value to wait for running work to complete infinitely.
    /// </remarks>
    /// <value>The graceful stop delay.</value>
    public TimeSpan GracefulStopDelay { get; set; } = DefaultGracefulStopDelay;
}