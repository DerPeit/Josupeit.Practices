#if !JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE_NO_ATTRIBUTES

namespace Microsoft.CodeAnalysis
{
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.All)]
    [global::System.Diagnostics.ConditionalAttribute("JOSUPEIT_PRACTICES_MIXINS_DISPOSABLE")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Josupeit.Practices.Mixins.Disposable.Generator", "1.0.0.0")]
    [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
    internal sealed class EmbeddedAttribute : global::System.Attribute
    {
    }
}

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