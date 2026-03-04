using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents an instance which has a particular progress.
/// </summary>
[PublicAPI]
public interface IHasProgress
{
    /// <summary>
    /// Gets the job's progress.
    /// </summary>
    /// <value>The progress.</value>
    object? Progress { get; }
}