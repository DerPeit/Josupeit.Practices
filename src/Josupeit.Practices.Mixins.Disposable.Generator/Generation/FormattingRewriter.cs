using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class FormattingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var @base = base.VisitClassDeclaration(node);

        if (@base is not ClassDeclarationSyntax { Members: var members } @class)
            return @base;

        return @class.WithMembers(List(members
            .OrderBy(x => x switch
            {
                FieldDeclarationSyntax => 0,
                ConstructorDeclarationSyntax => 1,
                DestructorDeclarationSyntax => 2,
                EventDeclarationSyntax => 3,
                EventFieldDeclarationSyntax => 3,
                PropertyDeclarationSyntax => 4,
                MethodDeclarationSyntax => 5,
                TypeDeclarationSyntax => 6,

                _ => throw new NotImplementedException(),
            })
            .ThenBy(x => x.Modifiers switch
            {
                { } m when m.Any(y => y.IsKind(SyntaxKind.PublicKeyword)) => 0,
                { } m when m.Any(y => y.IsKind(SyntaxKind.ProtectedKeyword)) && m.Any(y => y.IsKind(SyntaxKind.InternalKeyword)) => 1,
                { } m when m.Any(y => y.IsKind(SyntaxKind.InternalKeyword)) => 2,
                { } m when m.Any(y => y.IsKind(SyntaxKind.ProtectedKeyword)) => 3,
                { } m when m.Any(y => y.IsKind(SyntaxKind.PrivateKeyword)) && m.Any(y => y.IsKind(SyntaxKind.ProtectedKeyword)) => 4,
                _ => 99,
            })
            .ThenBy(x => x.Modifiers switch
            {
                { } m when m.Any(y => y.IsKind(SyntaxKind.StaticKeyword)) => 0,
                _ => 99,
            })
            .ThenBy(x => x.Modifiers switch
            {
                { } m when m.Any(y => y.IsKind(SyntaxKind.ReadOnlyKeyword)) => 0,
                _ => 99,
            })
            .ThenBy(x => x switch
            {
                BaseFieldDeclarationSyntax y => y.Declaration.Variables[0].Identifier.ToString(),
                EventDeclarationSyntax y => y.Identifier.ToString(),
                PropertyDeclarationSyntax y => y.Identifier.ToString(),
                MethodDeclarationSyntax y => y.Identifier.ToString(),
                TypeDeclarationSyntax y => y.Identifier.ToString(),

                _ => "",
            })
        ));
    }

    public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
    {
        if (node.Statement is BlockSyntax { Statements: [var statement] })
            node = node.WithStatement(statement);

        if (node.Else?.Statement is BlockSyntax { Statements: [var @else] })
            node = node.WithElse(ElseClause(@else));

        return base.VisitIfStatement(node);
    }
}