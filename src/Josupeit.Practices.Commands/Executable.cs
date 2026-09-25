// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This class represents artifacts that can be executed.
/// </summary>
[PublicAPI]
public abstract class Executable
{
    // Holds a marker while a bridged member of this instance is active in the current call chain.
    // The reset writes null on purpose: ExecutionContext removes an AsyncLocal entry only for null,
    // whereas a boxed false would stay in the flow's map for every instance ever executed.
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly AsyncLocal<object?> _recursionMarker = new();

    private protected IDisposable CreateExecuteScope()
    {
        if (_recursionMarker.Value is not null)
            throw new InvalidOperationException(
                "Neither the synchronous method Execute, nor the asynchronous overload " +
                "ExecuteAsync has been overridden or either base.Execute or base.ExecuteAsync " +
                "has been called from user-code which is prohibited. Please ensure to " +
                "override at least one of these methods and remove base calls and try again.");

        _recursionMarker.Value = this;

        return new ExecutionScope(this);
    }

    private sealed class ExecutionScope : IDisposable
    {
        private readonly Executable _owner;

        public ExecutionScope(Executable owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner._recursionMarker.Value = null;
        }
    }
}
