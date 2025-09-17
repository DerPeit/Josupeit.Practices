using System;
using Josupeit.Practices.Mixins.Disposable.Generator.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class ParentTypeWrappingRewriter(ParentType? parent) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (parent == null)
            return base.VisitClassDeclaration(node);

        var typeDeclaration = (TypeDeclarationSyntax)node;

        for (var enclosingTypeInfo = parent; enclosingTypeInfo != null; enclosingTypeInfo = enclosingTypeInfo.Parent)
        {
            var newTypeDeclaration = enclosingTypeInfo.Type switch
            {
                TypeDiscriminator.Class => (TypeDeclarationSyntax)ClassDeclaration(enclosingTypeInfo.NameWithConstraints),
                TypeDiscriminator.Struct => StructDeclaration(enclosingTypeInfo.NameWithConstraints),
                TypeDiscriminator.ClassRecord => RecordDeclaration(Token(SyntaxKind.RecordKeyword), enclosingTypeInfo.NameWithConstraints),
                TypeDiscriminator.StructRecord => RecordDeclaration(SyntaxKind.RecordStructDeclaration, Token(SyntaxKind.RecordKeyword), enclosingTypeInfo.NameWithConstraints).WithClassOrStructKeyword(Token(SyntaxKind.StructKeyword)),
                TypeDiscriminator.Interface => InterfaceDeclaration(enclosingTypeInfo.NameWithConstraints),

                _ => throw new InvalidOperationException()
            };

            newTypeDeclaration = newTypeDeclaration.AddModifiers(Token(SyntaxKind.PartialKeyword));
            newTypeDeclaration = newTypeDeclaration.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken));
            newTypeDeclaration = newTypeDeclaration.WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
            newTypeDeclaration = newTypeDeclaration.WithMembers(SingletonList<MemberDeclarationSyntax>(typeDeclaration));

            typeDeclaration = newTypeDeclaration;
        }

        return typeDeclaration;
    }
}