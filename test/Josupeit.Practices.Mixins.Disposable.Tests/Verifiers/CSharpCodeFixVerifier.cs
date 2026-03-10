// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Josupeit.Practices.Mixins.Disposable.Analyzer;
using Josupeit.Practices.Mixins.Disposable.Generator;

namespace Josupeit.Practices.Mixins.Disposable.Tests.Verifiers;

internal static class CSharpCodeFixVerifier<TCodeFix> where TCodeFix : CodeFixProvider, new()
{
    internal sealed class Test : CSharpCodeFixTest<DisposableAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestBehaviors |= TestBehaviors.SkipGeneratedSourcesCheck;
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp12;

        protected override ParseOptions CreateParseOptions()
            => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);

        protected override IEnumerable<Type> GetSourceGenerators()
        {
            yield return typeof(DisposableSourceGenerator);
        }
    }
}
