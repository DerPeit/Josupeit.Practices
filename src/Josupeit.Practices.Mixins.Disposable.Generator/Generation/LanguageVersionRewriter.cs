// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class LanguageVersionRewriter(LanguageVersion targetLanguageVersion) : CSharpSyntaxRewriter
{
    [return: NotNullIfNotNull("node")]
    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        if (targetLanguageVersion < LanguageVersion.CSharp10)
            node = new Lang10To9().Visit(node);

        if (targetLanguageVersion < LanguageVersion.CSharp8)
            node = new Lang8To73().Visit(node);

        if (targetLanguageVersion < LanguageVersion.CSharp6)
            node = new Lang6To5().Visit(node);

        return node;
    }
}

#pragma warning disable SA1402 // file-scoped types must share a file by definition.

file sealed class Lang10To9 : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        return VisitNamespaceDeclaration(
            NamespaceDeclaration(
                node.NamespaceKeyword,
                node.Name,
                Token(SyntaxKind.OpenBraceToken),
                node.Externs,
                node.Usings,
                node.Members,
                Token(SyntaxKind.CloseBraceToken),
                semicolonToken: default));
    }
}

file sealed class Lang8To73 : CSharpSyntaxRewriter
{
    public override bool VisitIntoStructuredTrivia => true;

    public override SyntaxNode? VisitNullableDirectiveTrivia(NullableDirectiveTriviaSyntax node)
    {
        return null;
    }

    public override SyntaxNode VisitNullableType(NullableTypeSyntax node)
    {
        return Visit(node.ElementType);
    }
}

file sealed class Lang6To5 : CSharpSyntaxRewriter
{
    private int _counter;

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (node.ExpressionBody != null)
            node = node
                .WithExpressionBody(null)
                .WithSemicolonToken(default)
                .WithAccessorList(
                    AccessorList(List([
                        AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration,
                            Block(ReturnStatement(node.ExpressionBody!.Expression)))
                    ]))
                );

        return base.VisitPropertyDeclaration(node);
    }

    public override SyntaxNode? VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        if (node.WhenNotNull is not InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax binding } invocation)
            return base.VisitConditionalAccessExpression(node);

        var variableName = Identifier($"value{Interlocked.Increment(ref _counter)}");
        var variableIdentifier = IdentifierName(variableName);

        var variableDeclaration = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(variableName)
                            .WithInitializer(EqualsValueClause(node.Expression))
                    )
                )
        );

        var ifStatement = IfStatement(
            BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                variableIdentifier,
                LiteralExpression(SyntaxKind.NullLiteralExpression)
            ),
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        variableIdentifier,
                        IdentifierName(binding.Name.Identifier)
                    ),
                    invocation.ArgumentList
                )
            )
        );

        return VisitBlock(Block(variableDeclaration, ifStatement));
    }

    public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        var expressionCandidate = Visit(node.Expression);

        if (expressionCandidate is ExpressionSyntax expression)
            return node.Update(VisitList(node.AttributeLists), expression, VisitToken(node.SemicolonToken));

        return expressionCandidate;
    }

    public override SyntaxNode? VisitBlock(BlockSyntax node)
    {
        var blockCandidate = base.VisitBlock(node);

        if (blockCandidate is not BlockSyntax block)
            return blockCandidate;

        return block.WithStatements(List(
            block.Statements.SelectMany(stmt =>
            {
                if (stmt is BlockSyntax nestedBlock)
                    return nestedBlock.Statements;

                return [stmt];
            })
        ));
    }

    public override SyntaxNode? VisitTryStatement(TryStatementSyntax node)
    {
        if (node.Finally == null)
            return base.VisitTryStatement(node);

        var awaitInFinally = node.Finally.DescendantNodes().Any(x => x is AwaitExpressionSyntax);
        if (!awaitInFinally)
            return base.VisitTryStatement(node);

        var edi = Identifier($"edi{Interlocked.Increment(ref _counter)}");
        var ediName = IdentifierName(edi);

        var ediDeclaration = LocalDeclarationStatement(
            VariableDeclaration(
                    AliasQualifiedName(IdentifierName(Token(SyntaxKind.GlobalKeyword)), IdentifierName(typeof(ExceptionDispatchInfo).FullName!))
                )
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(edi).WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
                    )
                )
        );

        var ex = Identifier("ex");

        var catchClause = CatchClause(
            CatchDeclaration(AliasQualifiedName(IdentifierName(Token(SyntaxKind.GlobalKeyword)), IdentifierName(typeof(Exception).FullName!)), ex),
            filter: null,
            Block(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    ediName,
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            AliasQualifiedName(
                                IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                                IdentifierName(typeof(ExceptionDispatchInfo).FullName!)
                            ),
                            IdentifierName(nameof(ExceptionDispatchInfo.Capture))
                        ),
                        ArgumentList(SingletonSeparatedList(Argument(IdentifierName(ex))))
                    )
                )
            ))
        );

        var tryBlock = node.Catches.Count > 0
            ? Block((StatementSyntax)base.VisitTryStatement(node.WithFinally(null))!)
            : (BlockSyntax)VisitBlock(node.Block)!;

        var tryStatement = TryStatement(tryBlock, SingletonList(catchClause), @finally: null);

        var conditionalThrowStatement = IfStatement(
            condition: BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                ediName,
                LiteralExpression(SyntaxKind.NullLiteralExpression)
            ),
            statement: ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ediName,
                        IdentifierName(nameof(ExceptionDispatchInfo.Throw))
                    ),
                    ArgumentList()
                )
            )
        );

        return Block(List<StatementSyntax>([ediDeclaration, tryStatement, ..node.Finally.Block.Statements, conditionalThrowStatement]));
    }
}
