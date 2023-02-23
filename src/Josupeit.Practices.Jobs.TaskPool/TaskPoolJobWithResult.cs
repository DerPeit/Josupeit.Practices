using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This class represents a job with a result that executes on the task pool.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.JobWithResult{TResult}" />
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.JobWithResult{TResult}" />
public class TaskPoolJobWithResult<TResult> : JobWithResult<TResult>
{
    private readonly Func<CancellationToken, Task<TResult>> _startFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolJobWithResult{TResult}"/> class.
    /// </summary>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="startFunc"/> must not be <c>null</c>.</exception>
    public TaskPoolJobWithResult(Func<CancellationToken, Task<TResult>> startFunc)
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

        TResult result;

        try
        {
            result = await _startFunc(CancellationToken).ConfigureAwait(false);
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