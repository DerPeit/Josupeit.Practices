using System.Threading.Tasks;

namespace Josupeit.Practices.Specifications;

internal static class Singletons
{
    public static readonly Task<bool> True = Task.FromResult(true);
    public static readonly Task<bool> False = Task.FromResult(false);
}