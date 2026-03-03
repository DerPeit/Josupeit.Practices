namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record ParentType(
    ParentType? Parent,
    TypeDiscriminator Type,
    string NameWithConstraints);
