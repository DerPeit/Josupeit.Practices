using System;
using System.Diagnostics;

namespace Josupeit.Practices.Commands;

/// <summary>
/// This class represents artifacts that can be executed.
/// </summary>
public abstract class Executable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private volatile bool _isRecursing;

    private protected IDisposable CreateExecuteScope()
    {
        var isRecursing = _isRecursing;
        _isRecursing = true;

        if (isRecursing)
            throw new InvalidOperationException(
                "Neither the synchronous method Execute, nor the asynchronous overload " +
                "ExecuteAsync has been overridden or either base.Execute or base.ExecuteAsync " +
                "has been called from user-code which is prohibited. Please ensure to " +
                "override at least one of these methods and remove base calls and try again.");

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
            _owner._isRecursing = false;
        }
    }
}