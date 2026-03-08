// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using JetBrains.Annotations;

namespace Josupeit.Practices.Jobs;

/// <summary>
/// This interface represents a job with a result.<para />
/// Implements the <see cref="Josupeit.Practices.Jobs.IJob" />
/// Implements the <see cref="Josupeit.Practices.Jobs.IHasResult{TResult}" />
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <seealso cref="Josupeit.Practices.Jobs.IJob" />
/// <seealso cref="Josupeit.Practices.Jobs.IHasResult{TResult}" />
[PublicAPI]
public interface IJobWithResult<out TResult> : IJob, IHasResult<TResult>
{
}
