namespace OpeaLibrary.Domain.Tests.Entities;

public class EntityTests
{
    private sealed class TestEntity : Entity
    {
        public TestEntity(Guid id) => Id = id;
    }

    private sealed class OtherTestEntity : Entity
    {
        public OtherTestEntity(Guid id) => Id = id;
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var entity = new TestEntity(Guid.NewGuid());

        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void Equals_NullOther_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());

        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        Entity entity = new TestEntity(id);
        Entity other = new OtherTestEntity(id);

        Assert.False(entity.Equals(other));
    }

    [Fact]
    public void Equals_SameTypeDifferentId_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var other = new TestEntity(Guid.NewGuid());

        Assert.False(entity.Equals(other));
    }

    [Fact]
    public void Equals_SameTypeSameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        var other = new TestEntity(id);

        Assert.True(entity.Equals(other));
    }

    [Fact]
    public void Equals_NonEntityObject_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());

        Assert.False(entity.Equals(new object()));
    }

    [Fact]
    public void GetHashCode_SameTypeSameId_ReturnsSameHashCode()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        var other = new TestEntity(id);

        Assert.Equal(entity.GetHashCode(), other.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        Entity? left = null;
        Entity? right = null;

        Assert.True(left == right);
    }

    [Fact]
    public void EqualityOperator_OneNull_ReturnsFalse()
    {
        Entity? left = new TestEntity(Guid.NewGuid());
        Entity? right = null;

        Assert.False(left == right);
        Assert.False(right == left);
    }

    [Fact]
    public void EqualityOperator_SameTypeSameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        Entity left = new TestEntity(id);
        Entity right = new TestEntity(id);

        Assert.True(left == right);
    }

    [Fact]
    public void InequalityOperator_DifferentId_ReturnsTrue()
    {
        Entity left = new TestEntity(Guid.NewGuid());
        Entity right = new TestEntity(Guid.NewGuid());

        Assert.True(left != right);
    }
}
