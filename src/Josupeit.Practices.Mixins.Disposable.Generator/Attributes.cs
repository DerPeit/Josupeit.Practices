// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

#if !JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE_NO_ATTRIBUTES && !JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE_NO_EMBEDDED_ATTRIBUTE

namespace Microsoft.CodeAnalysis
{
    // Deliberately declared bare. Roslyn recognizes this type by full name and marks it embedded
    // on its own, so no attribute of any kind is needed here - and every attribute that could be
    // added is AllowMultiple = false, which would produce CS0579 as soon as another generator
    // (PolySharp, for example) declares its own partial half of this type.
    internal sealed partial class EmbeddedAttribute : global::System.Attribute
    {
    }
}

#endif

#if !JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE_NO_ATTRIBUTES

namespace Josupeit.Practices.Mixins.Disposable
{
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct)]
    [global::System.Diagnostics.ConditionalAttribute("JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Josupeit.Practices.Mixins.Disposable.Generator", "1.0.0.0")]
    [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
    internal sealed class DisposableMixinAttribute : global::System.Attribute
    {
        public DisposableMixinAttribute()
        {
        }

        public DisposableMixinAttribute(DisposableMixinOptions options)
        {
            Options = options;
        }

        public DisposableMixinOptions Options { get; set; }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Josupeit.Practices.Mixins.Disposable.Generator", "1.0.0.0")]
    [global::System.FlagsAttribute]
    [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
    internal enum DisposableMixinOptions
    {
        None = 0,
        WithProperty = 1,
        WithEvent = 2,
        WithoutIdempotencyGuarantees = 4,
        WithoutAsyncDisposable = 8,
    }
}

#endif
