using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class ProtectedMemberDegradingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var newModifiers = new List<SyntaxToken>(capacity: node.Modifiers.Count);
        var addPrivateModifier = true;

        foreach (var modifier in node.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PublicKeyword) || modifier.IsKind(SyntaxKind.InternalKeyword) || modifier.IsKind(SyntaxKind.PrivateKeyword))
                addPrivateModifier = false;

            if (modifier.IsKind(SyntaxKind.ProtectedKeyword) || modifier.IsKind(SyntaxKind.VirtualKeyword))
                continue;

            newModifiers.Add(modifier);
        }

        if (addPrivateModifier)
            newModifiers.Insert(0, Token(SyntaxKind.PrivateKeyword));

        return base.VisitMethodDeclaration(node.WithModifiers(TokenList(newModifiers)));
    }
}