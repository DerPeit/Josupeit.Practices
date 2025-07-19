using System;

namespace Josupeit.Practices.Disposables.Mixins;

internal static class IndentingStringBuilderExtensions
{
    public static EndBlockAction AppendBlock(this IndentingStringBuilder sb)
    {
        sb.AppendLine("{");

        return new EndBlockAction(sb, sb.Indent());
    }

    public readonly ref struct EndBlockAction : IDisposable
    {
        private readonly IndentingStringBuilder _builder;
        private readonly IndentingStringBuilder.UnindentAction _unindentAction;

        public EndBlockAction(IndentingStringBuilder builder, IndentingStringBuilder.UnindentAction unindentAction)
        {
            _builder = builder;
            _unindentAction = unindentAction;
        }

        public void Dispose()
        {
            if (_builder == null)
                return;

            _unindentAction.Dispose();

            _builder.AppendLine("}");
        }
    }
}