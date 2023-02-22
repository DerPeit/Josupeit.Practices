namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents an instance which has a particular progress.
/// </summary>
public interface IHasProgress
{
    /// <summary>
    /// Gets the job's progress.
    /// </summary>
    /// <value>The progress.</value>
    object? Progress { get; }
}