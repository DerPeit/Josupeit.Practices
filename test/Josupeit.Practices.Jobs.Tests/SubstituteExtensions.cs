using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Josupeit.Practices.Jobs.Tests;

internal static class SubstituteExtensions
{
    public static object? InvokeMember(this object target, string name, params object[] args)
    {
        var targetType = target.GetType();
        var parameterTypes = args.Select(a => a.GetType()).ToArray();

        MethodInfo? method = null;

        if (parameterTypes is [])
        {
            method = targetType
                .GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
                .GetGetMethod(nonPublic: true);
        }

        method ??= targetType.GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            parameterTypes,
            modifiers: null);

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
