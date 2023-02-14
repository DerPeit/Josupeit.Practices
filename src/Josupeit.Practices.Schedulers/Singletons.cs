using System;
using System.Linq.Expressions;
using System.Threading;

namespace Josupeit.Practices.Schedulers;

internal static class Singletons
{
    public static readonly ExecutionContext DefaultExecutionContext 
        = Expression
            .Lambda<Func<ExecutionContext>>(Expression.Field(null, typeof(ExecutionContext), "Default"))
            .Compile()
            .Invoke();
}