using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents a job factory.
/// </summary>
[PublicAPI]
public interface IJobFactory
{
    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJob Create(Func<Task> startFunc);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJob Create(Func<CancellationToken, Task> startFunc);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <typeparam name="TProgress">The type of the progress.</typeparam>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJobWithProgress<TProgress> Create<TProgress>(Func<IProgress<TProgress>, Task> startFunc);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <typeparam name="TProgress">The type of the progress.</typeparam>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJobWithProgress<TProgress> Create<TProgress>(Func<IProgress<TProgress>, CancellationToken, Task> startFunc);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJobWithResult<TResult> Create<TResult>(Func<Task<TResult>> startFunc);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJobWithResult<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> startFunc);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <typeparam name="TProgress">The type of the progress.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJobWithProgressAndResult<TProgress, TResult> Create<TProgress, TResult>(Func<IProgress<TProgress>, Task<TResult>> startFunc);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <typeparam name="TProgress">The type of the progress.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="startFunc">The start function that is being invoked when the job is started.</param>
    /// <returns>The job.</returns>
    IJobWithProgressAndResult<TProgress, TResult> Create<TProgress, TResult>(Func<IProgress<TProgress>, CancellationToken, Task<TResult>> startFunc);
}