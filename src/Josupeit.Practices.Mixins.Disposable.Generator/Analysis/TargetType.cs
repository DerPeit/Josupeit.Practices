namespace Josupeit.Practices.Mixins.Disposable.Generator.Analysis;

internal sealed record TargetType(
    string Namespace,
    TypeDiscriminator Type,
    string Name,
    string NameWithConstraints);