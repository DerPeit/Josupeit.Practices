// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nito.AsyncEx;

namespace Josupeit.Practices.Schedulers;

#pragma warning disable CA1001 // Disposal is handled by the base class AsyncSchedulerService lifecycle.
#pragma warning disable CA1849 // Cancel() is deliberate; CancelAsync() does not exist on netstandard2.0.

/// <summary>
/// This class represents a scheduler that uses a <see cref="System.Threading.Channels.Channel"/> to execute
/// scheduled items eventually.<para />
/// Implements the <see cref="Josupeit.Practices.Schedulers.AsyncSchedulerService" />.
/// </summary>
/// <seealso cref="Josupeit.Practices.Schedulers.AsyncSchedulerService" />
[PublicAPI]
public class ChannelScheduler : AsyncSchedulerService
{
    private readonly AsyncMonitor _pauseMonitor = new();
    private readonly ChannelSchedulerOptions _options;

    private bool _isPausing;
    private volatile bool _isStopping;

#nullable disable
    private Channel<IScheduled> _channel;
    private Task _consumers;
    private CancellationTokenSource _stopping;
#nullable restore

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelScheduler"/> class.
    /// </summary>
    /// <param name="options">The options to configure the scheduler.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="options"/> must not be <c>null</c>.</exception>
    public ChannelScheduler(ChannelSchedulerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public override bool CanPause => true;

    /// <inheritdoc />
    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        var degreeOfParallelism = _options.DegreeOfParallelism;

        if (degreeOfParallelism <= 0)
            throw new InvalidOperationException("Configured degree of parallelism must be greater than zero.");

        _channel = Channel.CreateUnbounded<IScheduled>(
            new()
            {
                SingleWriter = false,
                SingleReader = degreeOfParallelism == 1,
            });

        _stopping = new();
        _isStopping = false;

        var thisPlusStoppingToken = new object[] { this, _stopping.Token };

        _consumers = Task.WhenAll(
            Enumerable
                .Range(0, degreeOfParallelism)
                .Select(_ => Task.Factory
                    .StartNew(
                        ConsumeAsync,
                        state: thisPlusStoppingToken,
                        CancellationToken.None,
                        TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning,
                        TaskScheduler.Default)
                    .Unwrap()));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task OnPauseAsync(CancellationToken cancellationToken)
    {
        using (await _pauseMonitor.EnterAsync(cancellationToken).ConfigureAwait(false))
        {
            _isPausing = true;
        }
    }

    /// <inheritdoc />
    protected override async Task OnResumeAsync(CancellationToken cancellationToken)
    {
        using (await _pauseMonitor.EnterAsync(cancellationToken).ConfigureAwait(false))
        {
            _isPausing = false;
            _pauseMonitor.PulseAll();
        }
    }

    /// <inheritdoc />
    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
        // From here on a consumer that dequeues an item declines it instead of running it: the drain
        // below races consumers that are between their read and their execute.
        _isStopping = true;

        // We close the channel so consumers end gracefully.
        _channel.Writer.Complete();

        // Next, we decline everything that is currently dangling, before paused consumers are released
        // and could dequeue it themselves.
        while (_channel.Reader.TryRead(out var scheduled))
            scheduled.Decline();

        // However, we need to unblock them if we're currently paused. Otherwise, they keep hanging.
        using (await _pauseMonitor.EnterAsync(cancellationToken).ConfigureAwait(false))
            if (_isPausing)
            {
                _isPausing = false;
                _pauseMonitor.PulseAll();
            }

        Timer? gracefulStopTimeout = null;

        switch (_options.GracefulStopDelay)
        {
            case { TotalMilliseconds: > 0 } dueTime:
                // We have a delay greater than zero, so we start a timer to cancel pending work. The
                // token source travels as the timer state: on netstandard2.0, Timer.Dispose does not
                // wait for a callback that is already running, and the field is cleared right after.
                gracefulStopTimeout = new Timer(
                    CancelRunningWork,
                    state: _stopping,
                    dueTime,
                    period: TimeSpan.FromMilliseconds(-1) /* no periodic signaling */);

                break;

            case { TotalMilliseconds: 0 }:
                // Delay is set to zero so we cancel pending work immediately.
                _stopping.Cancel();

                break;

            default:
                // Delay is smaller than zero so we wait infinitely.
                break;
        }

        try
        {
            await _consumers.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Exceptions due to cancellation are being discarded purposely. A running item that honors
            // the stop token with ThrowIfCancellationRequested surfaces the base type, not
            // TaskCanceledException, so the base type is what has to be caught here.
        }
        finally
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if (gracefulStopTimeout != null)
                await gracefulStopTimeout.DisposeAsync().ConfigureAwait(false);
#else
            gracefulStopTimeout?.Dispose();
#endif
        }

        // The default branch (infinite wait) never cancels the CTS, so Dispose is needed to release it.
        _stopping.Dispose();

        _channel = null;
        _consumers = null;
        _stopping = null;
    }

    /// <inheritdoc />
    protected override Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scheduled);

        ThrowIfStopped();

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        // Read the field once: a stop that begins right after the state check above completes the
        // writer and, once it has run to the end, clears the field. The channel is unbounded, so
        // TryWrite fails only for a completed writer. Decline the item in both cases instead of leaking
        // ChannelClosedException or NullReferenceException to the caller.
        var channel = _channel;

        if (channel is null || !channel.Writer.TryWrite(scheduled))
            scheduled.Decline();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Consume items from the channel as an asynchronous operation.
    /// </summary>
    /// <remarks>
    /// This method is called when the scheduler is being started, once for every configured
    /// <see cref="ChannelSchedulerOptions.DegreeOfParallelism"/>.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task.</returns>
    protected virtual async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var reader = _channel.Reader;
        var completion = reader.Completion;

        while (true)
        {
            // If the channel has already been closed there is no need to wait to read.
            if (completion.Status == TaskStatus.RanToCompletion)
                return;

            var readyToRead = reader.WaitToReadAsync(cancellationToken).AsTask();

            // Whatever happens first, completion or new data, will fire.
            var completionOrReadyToRead = await Task.WhenAny(completion, readyToRead).ConfigureAwait(false);

            if (ReferenceEquals(completionOrReadyToRead, completion))
                return;

            // We're ready to read from the channel, but need to wait if the scheduler is currently pausing.
            using (await _pauseMonitor.EnterAsync(cancellationToken).ConfigureAwait(false))
                while (_isPausing)
                    await _pauseMonitor.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Possibly, resume has been triggered by a stop request. So we need to check that again.
            if (completion.Status == TaskStatus.RanToCompletion)
                return;

            // Since it could have taken some time after pausing, we might need to start over if there is no item in the channel.
            if (!reader.TryRead(out var scheduled))
                continue;

            // A stop that began after the item was queued declines everything still queued; an item we
            // dequeued in that window is ours to decline.
            if (_isStopping)
            {
                scheduled.Decline();

                continue;
            }

            await scheduled.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static Task ConsumeAsync(object? state)
    {
        var arguments = (object[])state!;
        var @this = (ChannelScheduler)arguments[0];
        var cancellationToken = (CancellationToken)arguments[1];

        return @this.ConsumeAsync(cancellationToken);
    }

    private static void CancelRunningWork(object? state)
    {
        var stopping = (CancellationTokenSource)state!;

        try
        {
            stopping.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The consumers finished in the same instant the grace period expired, and the stop path
            // disposed the source before this callback reached it. Nothing is left to cancel.
        }
    }
}
