using System;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

[Flags]
internal enum TypeFlags
{
    None = 0,
    Sealed = 1,
    HasDisposableBase = 2,
}