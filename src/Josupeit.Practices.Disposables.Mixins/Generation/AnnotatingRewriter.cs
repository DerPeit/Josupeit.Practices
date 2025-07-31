using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Disposables.Mixins.Generation;

internal sealed class AnnotatingRewriter(string tool, string version) : CSharpSyntaxRewriter
{
    private static readonly AttributeSyntax _debuggerNonUserCodeAttribute = Attribute(
        AliasQualifiedName(
            IdentifierName(Token(SyntaxKind.GlobalKeyword)),
            IdentifierName(typeof(DebuggerNonUserCodeAttribute).FullName!)
        ));

    private static readonly AttributeSyntax _editorBrowsableAttribute = Attribute(
        AliasQualifiedName(
            IdentifierName(Token(SyntaxKind.GlobalKeyword)),
            IdentifierName(typeof(EditorBrowsableAttribute).FullName!)
        ),
        AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                AliasQualifiedName(
                    IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                    IdentifierName(typeof(EditorBrowsableState).FullName!)
                ),
                IdentifierName(nameof(EditorBrowsableState.Never))
            )
        )))
    );

    private static readonly AttributeSyntax _debuggerBrowsableAttribute = Attribute(
        AliasQualifiedName(
            IdentifierName(Token(SyntaxKind.GlobalKeyword)),
            IdentifierName(typeof(DebuggerBrowsableAttribute).FullName!)
        ),
        AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                AliasQualifiedName(
                    IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                    IdentifierName(typeof(DebuggerBrowsableState).FullName!)
                ),
                IdentifierName(nameof(DebuggerBrowsableState.Never))
            )
        )))
    );

    private static readonly AttributeSyntax _browsableAttribute = Attribute(
        AliasQualifiedName(
            IdentifierName(Token(SyntaxKind.GlobalKeyword)),
            IdentifierName(typeof(BrowsableAttribute).FullName!)
        ),
        AttributeArgumentList(SingletonSeparatedList(AttributeArgument(LiteralExpression(SyntaxKind.FalseLiteralExpression))))
    );

    private readonly AttributeSyntax _generatedCodeAttribute = Attribute(
        AliasQualifiedName(
            IdentifierName(Token(SyntaxKind.GlobalKeyword)),
            IdentifierName(typeof(GeneratedCodeAttribute).FullName!)
        ),
        AttributeArgumentList(SeparatedList([
            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(tool))),
            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(version)))
        ]))
    );

    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        return base.VisitFieldDeclaration(
            node.WithoutLeadingTrivia()
                .AddAttributeLists(
                    Encapsulate(
                        _generatedCodeAttribute,
                        _editorBrowsableAttribute,
                        _debuggerBrowsableAttribute,
                        _browsableAttribute))
                .WithLeadingTrivia(node.GetLeadingTrivia()));
    }

    public override SyntaxNode? VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        return base.VisitDestructorDeclaration(
            node
                .WithoutLeadingTrivia()
                .AddAttributeLists(
                    Encapsulate(
                        _generatedCodeAttribute,
                        _debuggerNonUserCodeAttribute,
                        _editorBrowsableAttribute,
                        _browsableAttribute))
                .WithLeadingTrivia(node.GetLeadingTrivia()));
    }

    public override SyntaxNode? VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        return base.VisitEventFieldDeclaration(
            node
                .WithoutLeadingTrivia()
                .AddAttributeLists(Encapsulate(_generatedCodeAttribute))
                .WithLeadingTrivia(node.GetLeadingTrivia()));
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        return base.VisitPropertyDeclaration(
            node
                .WithoutLeadingTrivia()
                .AddAttributeLists(
                    Encapsulate(
                        _generatedCodeAttribute,
                        _debuggerNonUserCodeAttribute))
                .WithLeadingTrivia(node.GetLeadingTrivia()));
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var leadingTrivia = node.GetLeadingTrivia();

        node = node
            .WithoutLeadingTrivia()
            .AddAttributeLists(
                Encapsulate(
                    _generatedCodeAttribute,
                    _debuggerNonUserCodeAttribute));

        if (node.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword)))
            node = node
                .AddAttributeLists(
                    Encapsulate(
                        _editorBrowsableAttribute,
                        _browsableAttribute));

        return base.VisitMethodDeclaration(node.WithLeadingTrivia(leadingTrivia));
    }

    private static AttributeListSyntax[] Encapsulate(params AttributeSyntax[] attributes)
    {
        return Array.ConvertAll(
            attributes,
            static attribute => AttributeList(SingletonSeparatedList(attribute)));
    }
}