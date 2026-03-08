// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// This class represents an exception that is thrown when a scheduler declines execution of a scheduled item.<para />
/// Implements the <see cref="InvalidOperationException" />
/// </summary>
/// <seealso cref="InvalidOperationException" />
[PublicAPI]
public class ExecutionDeclinedException : InvalidOperationException
{
    private const string MESSAGE = "After enqueuing, but prior to execution, the scheduler declined to process the operation. " +
                                   "Likely, the scheduler has transitioned to a state that no longer allows further processing. " +
                                   "It may have been stopped in the meantime.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionDeclinedException"/> class.
    /// </summary>
    public ExecutionDeclinedException()
        : base(MESSAGE)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionDeclinedException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception.</param>
    public ExecutionDeclinedException(Exception innerException)
        : base(MESSAGE, innerException)
    {
    }
}
