// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents an instance which has a particular result.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.IHasResult" />
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.IHasResult" />
[PublicAPI]
public interface IHasResult<out TResult> : IHasResult
{
    /// <summary>
    /// Gets the job's result.
    /// </summary>
    /// <value>The result.</value>
    new TResult? Result { get; }
}
