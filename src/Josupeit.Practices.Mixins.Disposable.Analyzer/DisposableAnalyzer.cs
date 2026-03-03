using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Josupeit.Practices.Mixins.Disposable.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            Diagnostics.TargetMustNotHaveDisposableBase,
            Diagnostics.TargetMustBePartial,
            Diagnostics.EnclosingTypeMustBePartial,
            Diagnostics.SealedTargetImplementsNoCallback,
            Diagnostics.TargetImplementsNoCallback,
            Diagnostics.TargetDoesNotImplementDisposeManagedResources,
            Diagnostics.TargetDoesNotImplementDisposeManagedResourcesAsync,
            Diagnostics.TargetDoesNotImplementDisposeUnmanagedResources,
            Diagnostics.MethodShouldBeProtectedVirtual,
            Diagnostics.MethodShouldBePrivate);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        if (TryGetClassSyntaxAndSemantic(context) is not var (classSyntax, classSemantics))
            return;

        if (TryGetAttributeSyntaxAndData(context, classSyntax, classSemantics) is not var (attributeSyntax, attributeData))
            return;

        var mixinOptions = GetMixinOptions(attributeData);
        var (disposableInterface, asyncDisposableInterface) = GetDisposableInterfaces(context);

        if (ReportTargetMustNotHaveDisposableBase(context, classSemantics, attributeSyntax, disposableInterface, asyncDisposableInterface))
            return;

        if (ReportTargetAndParentsMustBePartial(context, classSyntax))
            return;

        if (FindCallbacks(classSyntax, classSemantics, ["DisposeManagedResources", "DisposeManagedResourcesAsync", "DisposeUnmanagedResources"]) is not [var disposeManagedResources, var disposeManagedResourcesAsync, var disposeUnmanagedResources])
            throw new NotImplementedException();

        ReportMissingCallbacks(
            context,
            mixinOptions,
            classSyntax,
            classSemantics,
            asyncDisposableInterface, 
            disposeManagedResources, 
            disposeManagedResourcesAsync, 
            disposeUnmanagedResources);

        ReportInvalidVisibility(
            context,
            classSemantics,
            disposeManagedResources,
            disposeManagedResourcesAsync,
            disposeUnmanagedResources);
    }

    private static (ClassDeclarationSyntax Syntax, INamedTypeSymbol Semantics)? TryGetClassSyntaxAndSemantic(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ClassDeclarationSyntax classSyntax && context.SemanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken) is { } classSemantics)
            return (classSyntax, classSemantics);

        return null;
    }

    private static (AttributeSyntax Syntax, AttributeData Data)? TryGetAttributeSyntaxAndData(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classSyntax, INamedTypeSymbol classSemantics)
    {
        var attributeSyntax = classSyntax
            .AttributeLists
            .SelectMany(a => a.Attributes)
            .WithCancellation(context)
            .SingleOrDefault(a => context.SemanticModel.GetTypeInfo(a, context.CancellationToken).Type?.ToDisplayString() == typeof(DisposableMixinAttribute).FullName);

        if (attributeSyntax == null)
            return null;

        var attributeData = classSemantics
            .GetAttributes()
            .WithCancellation(context)
            .SingleOrDefault(a => a.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) == attributeSyntax);

        if (attributeData == null)
            return null;

        return (attributeSyntax, attributeData);
    }

    private static DisposableMixinOptions GetMixinOptions(AttributeData attributeData)
    {
        var mixinOptionsValue = 0;

        if (attributeData.NamedArguments.FirstOrDefault(x => x.Key == nameof(DisposableMixinAttribute.Options)) is { Value.Value: int ni })
            mixinOptionsValue = ni;
        else if (attributeData.ConstructorArguments.FirstOrDefault(x => x.Type?.Name == "DisposableMixinOptions") is { Value: int ci })
            mixinOptionsValue = ci;

        var mixinOptions = (DisposableMixinOptions)mixinOptionsValue;
        return mixinOptions;
    }

    private static (INamedTypeSymbol Disposable, INamedTypeSymbol? AsyncDisposable) GetDisposableInterfaces(SyntaxNodeAnalysisContext context)
    {
        var disposableInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("System.IDisposable")!;
        var asyncDisposableInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("System.IAsyncDisposable");

        return (disposableInterface, asyncDisposableInterface);
    }

    private static bool ReportTargetMustNotHaveDisposableBase(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSemantics, AttributeSyntax attributeSyntax, INamedTypeSymbol disposableInterface, INamedTypeSymbol? asyncDisposableInterface)
    {
        var attributeLocation = attributeSyntax.GetLocation();

        for (var baseType = classSemantics.BaseType; baseType != null && baseType.SpecialType != SpecialType.System_Object; baseType = baseType.BaseType)
        {
            var baseInterfaces = baseType.AllInterfaces;

            var baseImplementsDisposable = baseInterfaces.Contains(disposableInterface)
                                        || asyncDisposableInterface != null 
                                        && baseInterfaces.Contains(asyncDisposableInterface);

            if (!baseImplementsDisposable)
                continue;

            var diagnostic = Diagnostic.Create(
                Diagnostics.TargetMustNotHaveDisposableBase,
                attributeLocation,
                classSemantics.Name);

            context.ReportDiagnostic(diagnostic);
            return true;
        }

        return false;
    }

    private static bool ReportTargetAndParentsMustBePartial(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classSyntax)
    {
        for (SyntaxNode? node = classSyntax; node is TypeDeclarationSyntax typeDeclaration; node = typeDeclaration.Parent)
        {
            if (typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                continue;

            var diagnostic = Diagnostic.Create(
                typeDeclaration == classSyntax ? Diagnostics.TargetMustBePartial : Diagnostics.EnclosingTypeMustBePartial,
                typeDeclaration.Identifier.GetLocation(),
                typeDeclaration.Identifier.ToString());

            context.ReportDiagnostic(diagnostic);
            return true;
        }

        return false;
    }

    private static IReadOnlyList<MethodDeclarationSyntax?> FindCallbacks(ClassDeclarationSyntax classSyntax, INamedTypeSymbol classSemantics, IList<string> names)
    {
        var result = new MethodDeclarationSyntax?[names.Count];
        var count = 0;

        foreach (var member in classSyntax.Members)
        {
            if (member is not MethodDeclarationSyntax method)
                continue;

            if (method.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            if (names.IndexOf(method.Identifier.Text) is not (var i and > -1))
                continue;

            result[i] = method;

            if (++count == result.Length)
                break;
        }

        // The syntax-based search above only covers the current partial declaration.
        // For callbacks defined in a separate partial file, fall back to the semantic model
        // which sees all members across all partial declarations.
        for (var i = 0; i < names.Count; i++)
        {
            if (result[i] != null)
                continue;

            // Exclude methods marked with [GeneratedCode] to avoid matching the
            // generator's own stubs, which are part of the compilation at this point.
            var methodSymbol = classSemantics.GetMembers(names[i])
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => !m.IsStatic && !m.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.CodeDom.Compiler.GeneratedCodeAttribute"));

            if (methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax methodSyntax)
                result[i] = methodSyntax;
        }

        return result;
    }

    private static void ReportMissingCallbacks(SyntaxNodeAnalysisContext context, DisposableMixinOptions options, ClassDeclarationSyntax classSyntax, INamedTypeSymbol classSemantics, INamedTypeSymbol? asyncDisposableInterface, MethodDeclarationSyntax? disposeManagedResources, MethodDeclarationSyntax? disposeManagedResourcesAsync, MethodDeclarationSyntax? disposeUnmanagedResources)
    {
        var mustValidateAsync = asyncDisposableInterface != null && !options.HasFlag(DisposableMixinOptions.WithoutAsyncDisposable);

        var hasNoCallback
            = ReportMissingCallback(context, classSyntax, classSemantics, disposeManagedResources, Diagnostics.TargetDoesNotImplementDisposeManagedResources)
              & (!mustValidateAsync || ReportMissingCallback(context, classSyntax, classSemantics, disposeManagedResourcesAsync, Diagnostics.TargetDoesNotImplementDisposeManagedResourcesAsync))
              & ReportMissingCallback(context, classSyntax, classSemantics, disposeUnmanagedResources, Diagnostics.TargetDoesNotImplementDisposeUnmanagedResources);

        if (!hasNoCallback)
            return;

        var targetHasNoCallbackDiagnostic = Diagnostic.Create(
            classSyntax.Modifiers.Any(SyntaxKind.SealedKeyword) ? Diagnostics.SealedTargetImplementsNoCallback : Diagnostics.TargetImplementsNoCallback,
            classSyntax.Identifier.GetLocation(),
            classSemantics.Name);

        context.ReportDiagnostic(targetHasNoCallbackDiagnostic);
    }

    private static bool ReportMissingCallback(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classSyntax, INamedTypeSymbol classSemantics, MethodDeclarationSyntax? callbackSyntax, DiagnosticDescriptor diagnosticDescriptor)
    {
        if (callbackSyntax != null)
            return false;

        var diagnostic = Diagnostic.Create(diagnosticDescriptor, classSyntax.Identifier.GetLocation(), classSemantics.Name);

        context.ReportDiagnostic(diagnostic);
        return true;
    }

    private static void ReportInvalidVisibility(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSemantics, MethodDeclarationSyntax? disposeManagedResources, MethodDeclarationSyntax? disposeManagedResourcesAsync, MethodDeclarationSyntax? disposeUnmanagedResources)
    {
        ReportInvalidVisibility(context, classSemantics, disposeManagedResources);
        ReportInvalidVisibility(context, classSemantics, disposeManagedResourcesAsync);
        ReportInvalidVisibility(context, classSemantics, disposeUnmanagedResources);
    }

    private static void ReportInvalidVisibility(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSemantics, MethodDeclarationSyntax? callbackSyntax)
    {
        if (callbackSyntax == null)
            return;

        if (classSemantics.IsSealed)
        {
            if (HasOnlyVisibilityModifiers(callbackSyntax, SyntaxKind.PrivateKeyword))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.MethodShouldBePrivate,
                    callbackSyntax.Identifier.GetLocation(),
                    classSemantics.Name,
                    callbackSyntax.Identifier.ToString()));
        }
        else
        {
            if (HasOnlyVisibilityModifiers(callbackSyntax, SyntaxKind.ProtectedKeyword, SyntaxKind.VirtualKeyword))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.MethodShouldBeProtectedVirtual,
                    callbackSyntax.Identifier.GetLocation(),
                    classSemantics.Name,
                    callbackSyntax.Identifier.ToString()));
        }

        return;

        static bool HasOnlyVisibilityModifiers(MethodDeclarationSyntax callbackSyntax, params SyntaxKind[] mustHaves)
        {
            var mustNotHaves = new[] { SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.PrivateKeyword, SyntaxKind.VirtualKeyword }.Except(mustHaves);

            foreach (var modifier in mustHaves)
                if (!callbackSyntax.Modifiers.Any(candidate => candidate.IsKind(modifier)))
                    return false;

            foreach (var modifier in mustNotHaves)
                if (callbackSyntax.Modifiers.Any(candidate => candidate.IsKind(modifier)))
                    return false;

            return true;
        }
    }
}
