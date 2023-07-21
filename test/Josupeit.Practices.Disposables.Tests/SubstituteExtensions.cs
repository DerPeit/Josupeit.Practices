using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Josupeit.Practices.Disposables.Tests;

internal static class SubstituteExtensions
{
    public static object? Protected(this object target, string name, params object[] args)
    {
        var targetType = target.GetType();
        var parameterTypes = args.Select(a => a.GetType()).ToArray();

        var method = targetType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic, binder: null, parameterTypes, modifiers: null);

        try
        {
            return method!.Invoke(target, args);
        }
        catch (TargetInvocationException ex)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();

            throw;
        }
    }
}