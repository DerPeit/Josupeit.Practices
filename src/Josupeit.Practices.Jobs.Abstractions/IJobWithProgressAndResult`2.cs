namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents a job with a progress and a result.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.IJobWithProgress{TProgress}" />
/// Implements the <see cref="Josupeit.Practices.Jobs.IJobWithResult{TResult}" />
/// </summary>
/// <typeparam name="TProgress">The type of the progress.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.IJobWithProgress{TProgress}" />
/// <seealso cref="Josupeit.Practices.Jobs.IJobWithResult{TResult}" />
public interface IJobWithProgressAndResult<out TProgress, out TResult> : IJobWithProgress<TProgress>, IJobWithResult<TResult>
{
}