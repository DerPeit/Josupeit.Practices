// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents an instance which has a particular result.
/// </summary>
[PublicAPI]
public interface IHasResult
{
    /// <summary>
    /// Gets the job's result.
    /// </summary>
    /// <value>The result.</value>
    object? Result { get; }
}
