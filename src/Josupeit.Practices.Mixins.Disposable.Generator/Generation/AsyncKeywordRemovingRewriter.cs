using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class AsyncKeywordRemovingRewriter : CSharpSyntaxRewriter
{
    // This class rewrites async ValueTask methods that don't utilize await with
    // return new ValueTask(). It does not work with nested local functions, lambdas, etc.
    // It also does not work with Tasks. It's tailored to the exact code. If this
    // is subject to change, this code must be adjusted.

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var isAsync = node.Modifiers.Any(SyntaxKind.AsyncKeyword);

        if (!isAsync)
            return base.VisitMethodDeclaration(node);

        Debug.Assert(node.ReturnType.ToFullString().Trim().EndsWith("ValueTask"), "Return type is no ValueTask.");

        var awaitChecker = new AwaitChecker();
        
        awaitChecker.Visit(node);

        if (awaitChecker.HasAwait)
            return base.VisitMethodDeclaration(node);

        var returnValue = ObjectCreationExpression(node.ReturnType, ArgumentList([]), initializer: null);
        var returnRewriter = new ReturnRewriter(returnValue);

        return returnRewriter.Visit(
            node
                .WithModifiers(TokenList(node.Modifiers.Where(x => !x.IsKind(SyntaxKind.AsyncKeyword))))
                .AddBodyStatements(ReturnStatement()));
    }
}

file sealed class AwaitChecker : CSharpSyntaxWalker
{
    private Stack<StrongBox<bool>>? _hasAwaits;

    public bool HasAwait { get; private set; }

    public override void Visit(SyntaxNode? node)
    {
        if (_hasAwaits != null)
        {
            base.Visit(node);
            return;
        }

        _hasAwaits = new Stack<StrongBox<bool>>();

        try
        {
            base.Visit(node);
        }
        finally
        {
            _hasAwaits = null;
        }
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        _hasAwaits!.Push(new StrongBox<bool>(false));

        try
        {
            base.VisitMethodDeclaration(node);
        }
        finally
        {
            HasAwait = _hasAwaits!.Pop().Value;
        }
    }

    public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        _hasAwaits!.Push(new StrongBox<bool>(false));

        try
        {
            base.VisitLocalFunctionStatement(node);
        }
        finally
        {
            HasAwait = _hasAwaits!.Pop().Value;
        }
    }

    public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
    {
        _hasAwaits!.Push(new StrongBox<bool>(false));

        try
        {
            base.VisitParenthesizedLambdaExpression(node);
        }
        finally
        {
            HasAwait = _hasAwaits!.Pop().Value;
        }
    }

    public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        _hasAwaits!.Push(new StrongBox<bool>(false));

        try
        {
            base.VisitSimpleLambdaExpression(node);
        }
        finally
        {
            HasAwait = _hasAwaits!.Pop().Value;
        }
    }

    public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        _hasAwaits!.Push(new StrongBox<bool>(false));

        try
        {
            base.VisitAnonymousMethodExpression(node);
        }
        finally
        {
            HasAwait = _hasAwaits!.Pop().Value;
        }
    }

    public override void VisitAwaitExpression(AwaitExpressionSyntax node)
    {
        _hasAwaits!.Peek().Value = true;

        base.VisitAwaitExpression(node);
    }
}

file sealed class ReturnRewriter(ExpressionSyntax expression) : CSharpSyntaxRewriter
{
    public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
    {
        return node.WithExpression(expression);
    }
}