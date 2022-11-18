using System.Threading.Tasks;
using Josupeit.Practices.Polyfills.Tasks;

namespace Josupeit.Practices.Specifications;

internal static class Singletons
{
    public static readonly Task<bool> True = TaskPolyfill.FromResult(true);
    public static readonly Task<bool> False = TaskPolyfill.FromResult(false);
}