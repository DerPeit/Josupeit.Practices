// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal interface IScheduledFunction<TResult> : IScheduled
{
    Task<TResult> Promise { get; }

    TResult WaitAndUnwrap();
}
