using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job with a progress and a result that executes on the task pool.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.Job" />
/// </summary>
/// <typeparam name="TProgress">The type of the progress.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.JobWithProgressAndResult{TProgress,TResult}" />
public class TaskPoolJobWithProgressAndResult<TProgress, TResult> : JobWithProgressAndResult<TProgress, TResult>
{
    private readonly Func<IProgress<TProgress>, CancellationToken, Task<TResult>> _startFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolJobWithProgressAndResult{TProgress, TResult}"/> class.
    /// </summary>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="startFunc"/> must not be <c>null</c>.</exception>
    public TaskPoolJobWithProgressAndResult(Func<IProgress<TProgress>, CancellationToken, Task<TResult>> startFunc)
    {
        _startFunc = startFunc ?? throw new ArgumentNullException(nameof(startFunc));
    }

    /// <inheritdoc />
    public override async void Start()
    {
        ThrowIfCannotStart();

        NotifyPending();

        await Task.Yield();

        NotifyExecuting();

        var progress = new Progress<TProgress>(NotifyProgress);

        TResult result;

        try
        {
            result = await _startFunc(progress, CancellationToken).ConfigureAwait(false);
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

        NotifySucceeded(result);
    }
}