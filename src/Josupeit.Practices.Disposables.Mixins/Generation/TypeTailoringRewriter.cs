using Josupeit.Practices.Disposables.Mixins.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Disposables.Mixins.Generation;

internal sealed class TypeTailoringRewriter(TargetType type) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        return base.VisitFileScopedNamespaceDeclaration(node.WithName(IdentifierName(type.Namespace)));
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