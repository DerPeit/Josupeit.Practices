using Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

namespace Josupeit.Practices.Mixins.Disposable.Generator.Generation;

internal sealed class PipelineFactory(SemanticTarget target, string tool, string version) : IPipelineFactory
{
    public RewritePipeline Create()
    {
        var pipeline = RewritePipeline.Empty;

        pipeline |= new TypeTailoringRewriter(target.Type);
        pipeline |= new AnnotatingRewriter(tool, version);

        if (!target.Options.HasFlag(MixinOptions.WithProperty))
            pipeline |= new PropertyRemovingRewriter();

        if (!target.Options.HasFlag(MixinOptions.WithEvent))
            pipeline |= new EventRemovingRewriter();

        if (target.Options.HasFlag(MixinOptions.WithoutAsyncDisposable))
            pipeline |= new AsyncDisposableRemovingRewriter();

        if (target.Options.HasFlag(MixinOptions.WithoutIdempotencyGuarantees))
            pipeline |= new IdempotencyGuaranteeRemovingRewriter();

        if (target.TypeProperties.Members.HasFlag(ImplementedMembers.DisposeManagedResources))
            pipeline |= new MethodRemovingRewriter(nameof(ImplementedMembers.DisposeManagedResources));

        if (target.TypeProperties.Members.HasFlag(ImplementedMembers.DisposeManagedResourcesAsync))
            pipeline |= new MethodRemovingRewriter(nameof(ImplementedMembers.DisposeManagedResourcesAsync));

        if (target.TypeProperties.Members.HasFlag(ImplementedMembers.DisposeUnmanagedResources))
            pipeline |= new MethodRemovingRewriter(nameof(ImplementedMembers.DisposeUnmanagedResources));

        if (target.TypeProperties.Flags.HasFlag(TypeFlags.Sealed))
            pipeline |= new ProtectedMemberDegradingRewriter();

        if (target.TypeProperties.Flags.HasFlag(TypeFlags.Sealed) && !target.TypeProperties.Members.HasFlag(ImplementedMembers.DisposeUnmanagedResources))
            pipeline |= new FinalizerRemovingRewriter();

        pipeline |= new StubRemovingRewriter();
        pipeline |= new EmptyTryBlockRewriter();
        pipeline |= new AsyncKeywordRemovingRewriter();
        pipeline |= new MarkerBlockInliningRewriter();
        pipeline |= new FormattingRewriter();
        pipeline |= new LanguageVersionRewriter(target.LanguageVersion);

        return pipeline;
    }
}