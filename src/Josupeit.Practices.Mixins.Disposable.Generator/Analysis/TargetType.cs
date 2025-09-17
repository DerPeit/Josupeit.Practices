namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record TargetType(
    string? Namespace,
    ParentType? Parent,
    TypeDiscriminator Type,
    string Name,
    string NameWithConstraints);
