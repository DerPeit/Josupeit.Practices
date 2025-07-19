#if !JOSUPEIT_MIXINS_DISPOSABLE_NO_ATTRIBUTES

namespace Josupeit.Practices.Mixins.Disposable
{
    [global::System.AttributeUsageAttribute(System.AttributeTargets.Class)]
    [global::System.Diagnostics.ConditionalAttribute("JOSUPEIT_MIXINS_DISPOSABLE")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Josupeit.Practices.Mixins.Disposable", "1.0.0.0")]
    internal sealed class DisposableMixinAttribute : global::System.Attribute
    {
        public DisposableMixinAttribute()
        {
        }

        public DisposableMixinAttribute(DisposableMixinBehavior behavior)
        {
            Behavior = behavior;
        }

        public DisposableMixinBehavior Behavior { get; set; }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Josupeit.Practices.Mixins.Disposable", "1.0.0.0")]
    internal enum DisposableMixinBehavior
    {
        Default,
        ImplementDisposable,
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET462_OR_GREATER
        ImplementAsyncDisposable,
        ImplementDisposableAndAsyncDisposable,
#endif
    }
}

#endif