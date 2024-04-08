#if NET40
extern alias Theraot;
#endif

using System.Linq;
using System.Reflection;

namespace Josupeit.Practices.Services.Tests;

internal static class SubstituteExtensions
{
    public static object? InvokeMember(this object target, string name, params object[] args)
    {
        var targetType = target.GetType();
        var parameterTypes = args.Select(a => a.GetType()).ToArray();

        MethodInfo? method = null;

        if (parameterTypes.Length == 0)
        {
            method = targetType
                .GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
                .GetGetMethod();
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
#if NET40
            Theraot.
#endif

            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();

            throw;
        }
    }
}