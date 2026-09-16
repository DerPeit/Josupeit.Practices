// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using JetBrains.Annotations;

using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace Josupeit.Practices.Jobs;

#pragma warning disable CA1034 // C# 14 extension block, not a traditional nested type.

/// <summary>
/// This class contains extension methods for an <seealso cref="IJob"/>.
/// </summary>
[PublicAPI]
public static class AwaitableExtensions
{
    extension(IJob job)
    {
        /// <summary>
        /// Gets the job awaiter.
        /// </summary>
        /// <returns>A new job awaiter. Each call allocates a new instance.</returns>
        /// <exception cref="System.ArgumentNullException">The job must not be <c>null</c>.</exception>
        [Pure]
        public IJobAwaiter GetAwaiter()
        {
            ArgumentNullException.ThrowIfNull(job);

            return new JobAwaiter(job);
        }

        /// <summary>
        /// Configures how awaits on the job are performed.
        /// </summary>
        /// <param name="continueOnCapturedContext">
        /// <c>true</c> to attempt to marshal the continuation back to the original context captured; otherwise, <c>false</c>.
        /// </param>
        /// <returns>An object used to await this job.</returns>
        /// <exception cref="System.ArgumentNullException">The job must not be <c>null</c>.</exception>
        [Pure]
        public ConfiguredJobAwaitable ConfigureAwait(bool continueOnCapturedContext)
        {
            ArgumentNullException.ThrowIfNull(job);

            return new ConfiguredJobAwaitable(job, continueOnCapturedContext);
        }

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
        /// <exception cref="System.ArgumentNullException">The job must not be <c>null</c>.</exception>
        public void Wait()
        {
            ArgumentNullException.ThrowIfNull(job);

            job.GetAwaiter().GetResult();
        }
    }
}
