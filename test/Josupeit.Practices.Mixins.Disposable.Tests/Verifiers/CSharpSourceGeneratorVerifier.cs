// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Josupeit.Practices.Mixins.Disposable.Generator;

namespace Josupeit.Practices.Mixins.Disposable.Tests.Verifiers;

internal static class CSharpSourceGeneratorVerifier
{
    internal sealed class Test : CSharpSourceGeneratorTest<DisposableSourceGenerator, DefaultVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp12;

        protected override ParseOptions CreateParseOptions()
            => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
    }
}
