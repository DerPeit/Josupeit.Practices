// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers.Tests;

[TestOf(typeof(AsyncScheduler)), UsedImplicitly]
public class AsyncSchedulerTests : SchedulerTests<AsyncScheduler>;
