using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job with a progress that executes on the task pool.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.JobWithProgress{TProgress}" />
/// </summary>
/// <typeparam name="TProgress">The type of the progress.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.JobWithProgress{TProgress}" />
public class TaskPoolJobWithProgress<TProgress> : JobWithProgress<TProgress>
{
    private readonly Func<IProgress<TProgress>, CancellationToken, Task> _startFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolJobWithProgress{TProgress}"/> class.
    /// </summary>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="startFunc"/> must not be <c>null</c>.</exception>
    public TaskPoolJobWithProgress(Func<IProgress<TProgress>, CancellationToken, Task> startFunc)
    {
        _startFunc = startFunc ?? throw new ArgumentNullException(nameof(startFunc));
    }

    /// <inheritdoc />
    public override async void Start()
    {
        ThrowIfCannotStart();

        NotifyPending();

        await Task.Yield();

        var progress = new Progress<TProgress>(NotifyProgress);

        NotifyExecuting();

        try
        {
            await _startFunc(progress, CancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException) when (CancellationToken.IsCancellationRequested)
        {
            NotifyCancelled();

            return;
        }
        catch (Exception ex)
        {
            NotifyFailed(ex);

            return;
        }

        NotifySucceeded();
    }
}