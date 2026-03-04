using JetBrains.Annotations;

namespace Josupeit.Practices.Services;

/// <summary>
/// This enum describes all states of a <see cref="IService"/>.
/// </summary>
[PublicAPI]
public enum ServiceState
{
    /// <summary>
    /// The referenced <see cref="IService"/> is being stopped.
    /// </summary>
    Stopping,

    /// <summary>
    /// The referenced <see cref="IService"/> is currently stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The referenced <see cref="IService"/> is currently being started.
    /// </summary>
    Starting,

    /// <summary>
    /// The referenced <see cref="IService"/> is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The referenced <see cref="IService"/> is currently being paused.
    /// </summary>
    Pausing,

    /// <summary>
    /// The referenced <see cref="IService"/> is currently paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The referenced <see cref="IService"/> is currently being resumed.
    /// </summary>
    Resuming
}