namespace Josupeit.Practices.Disposables.Mixins;

internal sealed record SemanticTarget(
    bool IsSealed,
    string TypeName,
    string Namespace,
    string TypeNameWithConstraints,
    bool WantsIDisposable,
    bool WantsIAsyncDisposable,
    bool HasDisposeManagedSyncCallback,
    bool HasDisposeManagedAsyncCallback,
    bool HasDisposeUnmanagedCallback);
