// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Josupeit.Practices.Mixins.Disposable.Generator.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class TypeTailoringRewriter(TargetType type) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        if (type.Namespace == null)
        {
            var namespaceDeclaration = (FileScopedNamespaceDeclarationSyntax)node.Members[0];

            node = node
                .WithAttributeLists(namespaceDeclaration.AttributeLists)
                .WithExterns(namespaceDeclaration.Externs)
                .WithMembers(namespaceDeclaration.Members)
                .WithUsings(namespaceDeclaration.Usings)
                .WithTriviaFrom(namespaceDeclaration);
        }

        return base.VisitCompilationUnit(node);
    }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        return base.VisitFileScopedNamespaceDeclaration(node.WithName(IdentifierName(type.Namespace!)));
    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        return base.VisitClassDeclaration(node.WithIdentifier(Identifier(type.NameWithConstraints)));
    }

    public override SyntaxNode? VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        return base.VisitDestructorDeclaration(node.WithIdentifier(Identifier(type.Name)));
    }
}
