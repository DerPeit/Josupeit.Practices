namespace Josupeit.Practices.Disposables.Mixins.Analysis;

internal sealed record TargetType(
    string Namespace,
    TypeDiscriminator Type,
    string Name,
    string NameWithConstraints);