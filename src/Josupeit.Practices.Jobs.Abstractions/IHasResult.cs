namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents an instance which has a particular result.
/// </summary>
public interface IHasResult
{
    /// <summary>
    /// Gets the job's result.
    /// </summary>
    /// <value>The result.</value>
    object? Result { get; }
}