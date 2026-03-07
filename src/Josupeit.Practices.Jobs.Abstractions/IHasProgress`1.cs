// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents an instance which has a particular progress.<para />
/// Implements <see cref="Josupeit.Practices.Jobs.IHasProgress" />
/// </summary>
/// <typeparam name="TProgress">The type of the progress.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.IHasProgress" />
[PublicAPI]
public interface IHasProgress<out TProgress> : IHasProgress
{
    /// <summary>
    /// Gets the job's progress.
    /// </summary>
    /// <value>The progress.</value>
    new TProgress? Progress { get; }
}
