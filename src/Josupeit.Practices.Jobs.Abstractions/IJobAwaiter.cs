// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents a job awaiter. It is used by the framework to support awaiting a job.
/// Implements the <see cref="ICriticalNotifyCompletion" />
/// </summary>
/// <seealso cref="ICriticalNotifyCompletion" />
[PublicAPI]
public interface IJobAwaiter : ICriticalNotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether this job is completed.
    /// </summary>
    /// <value><c>true</c> if this job is completed; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }

    /// <summary>
    /// Waits for the job to complete.
    /// </summary>
    /// <remarks>
    /// <para>This method does not throw if the job is canceled or faulted.</para>
    /// <para>
    /// This method blocks the calling thread. Do not call it from a context that the job
    /// needs in order to complete (e.g. a single-threaded <see cref="System.Threading.SynchronizationContext"/>),
    /// as this will result in a deadlock.
    /// </para>
    /// </remarks>
    void GetResult();
}
