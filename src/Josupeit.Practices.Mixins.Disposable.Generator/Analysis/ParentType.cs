namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record ParentType(ParentType? Parent, TypeDiscriminator Type, string NameWithConstraints)
{
    internal string GenerateHintName()
    {
        var parentHintName = Parent?.GenerateHintName();
        var selfHintName = NameWithConstraints.Replace('<', '[').Replace('>', ']').Replace(" ", "");

        return parentHintName == null ? selfHintName : string.Concat(parentHintName, '.', selfHintName);
    }
}