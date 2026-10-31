namespace SGCAE.Domain.Common;

/// <summary>
/// Objeto de valor: sin identidad, inmutable y comparado por sus componentes.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject other && other.GetType() == GetType() &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (hash, c) => HashCode.Combine(hash, c));

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
