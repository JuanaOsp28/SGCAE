namespace SGCAE.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity(Guid id) => Id = id;

    // Requerido por EF Core
    protected Entity() { }

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
