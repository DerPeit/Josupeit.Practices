// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.IO;
using System.Text;
using System.Threading;
using Josupeit.Practices.Mixins.Disposable.Generator.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal readonly ref struct CodeGenerator(SemanticTarget target, string tool, string version)
{
    public SourceText Generate(CancellationToken cancellationToken)
    {
        using var stream = typeof(CodeGenerator).Assembly.GetManifestResourceStream(typeof(CodeGenerator), "Template.cs")!;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);

        var syntaxTree = CSharpSyntaxTree.ParseText(reader.ReadToEnd(), encoding: reader.CurrentEncoding, cancellationToken: cancellationToken);
        var pipeline = new PipelineFactory(target, tool, version).Create();
        var syntaxNode = pipeline.Run(syntaxTree.GetRoot(cancellationToken));
        var sourceText = SourceText.From(syntaxNode.NormalizeWhitespace().ToFullString(), reader.CurrentEncoding);

        return sourceText;
    }
}
