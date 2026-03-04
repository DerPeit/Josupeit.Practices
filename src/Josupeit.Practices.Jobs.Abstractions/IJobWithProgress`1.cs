using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents a job with a progress.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.IJob" />
/// Implements the <see cref="Josupeit.Practices.Jobs.IHasProgress{TProgress}" />
/// </summary>
/// <typeparam name="TProgress">The type of the progress.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.IJob" />
/// <seealso cref="Josupeit.Practices.Jobs.IHasProgress{TProgress}" />
[PublicAPI]
public interface IJobWithProgress<out TProgress> : IJob, IHasProgress<TProgress>
{
}