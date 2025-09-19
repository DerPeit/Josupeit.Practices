using Microsoft.CodeAnalysis;

namespace Josupeit.Practices.Mixins.Disposable.Analyzer;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor TargetMustNotHaveDisposableBase = new(
        "JPMD010",
        title: "Class is already disposable",
        messageFormat: 
            "Class '{0}' derives from a class that already implements System.IDisposable or System.IAsyncDisposable (or both). " +
            "This scenario is unsupported. Use the provided hooks from the base class to dispose any resources owned by this class, " +
            "instead. The [DisposableMixin] has no effect.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TargetMustBePartial = new(
        "JPMD020",
        title: "Class must be partial",
        messageFormat: "Class '{0}' must be partial, because it is annotated with the [DisposableMixin] attribute",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EnclosingTypeMustBePartial = new(
        "JPMD021",
        title: "Type must be partial",
        messageFormat: "Type '{0}' must be partial, because it contains a nested class which is annotated with the [DisposableMixin] attribute",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SealedTargetImplementsNoCallback = new(
        "JPMD030",
        title: "Sealed class implements no callback",
        messageFormat: "Sealed class '{0}' neither implements DisposeManagedResources, DisposeManagedResourcesAsync nor DisposeUnmanagedResources",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TargetImplementsNoCallback = new(
        "JPMD031",
        title: "Class implements no callback",
        messageFormat: "Class '{0}' neither implements DisposeManagedResources, DisposeManagedResourcesAsync nor DisposeUnmanagedResources",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TargetDoesNotImplementDisposeManagedResources = new(
        "JPMD032",
        title: "Class does not implement DisposeManagedResources",
        messageFormat: "Class '{0}' does not implement DisposeManagedResources",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TargetDoesNotImplementDisposeManagedResourcesAsync = new(
        "JPMD033",
        title: "Class does not implement DisposeManagedResourcesAsync",
        messageFormat: "Class '{0}' does not implement DisposeManagedResourcesAsync",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TargetDoesNotImplementDisposeUnmanagedResources = new(
        "JPMD034",
        title: "Class does not implement DisposeUnmanagedResources",
        messageFormat: "Class '{0}' does not implement DisposeUnmanagedResources",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodShouldBeProtectedVirtual = new(
        "JPMD040",
        title: "Method should be protected virtual",
        messageFormat: "Method '{0}.{1}' should have protected visibility and be overridable",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodShouldBePrivate = new(
        "JPMD041",
        title: "Method should be private",
        messageFormat: "Method '{0}.{1}' should have private visibility",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}