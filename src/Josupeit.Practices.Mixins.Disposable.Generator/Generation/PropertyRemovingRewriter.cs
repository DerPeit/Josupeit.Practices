// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class PropertyRemovingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        return null;
    }
}
