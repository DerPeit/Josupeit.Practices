// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal interface IScheduledAction : IScheduled
{
    Task Promise { get; }

    void Wait();
}
