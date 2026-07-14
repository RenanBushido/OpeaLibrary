namespace OpeaLibrary.Domain.Entities;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; }

    public bool Equals(Entity? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (GetType() == other.GetType() && Id == other.Id));
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}