using Josupeit.Practices.Mixins.Disposable;

namespace Examples
{
    [DisposableMixin(DisposableMixinOptions.WithEvent | DisposableMixinOptions.WithProperty)]
    public partial class DisposableService<TTest>
    {
        public DisposableService()
        {
        }
    }
}