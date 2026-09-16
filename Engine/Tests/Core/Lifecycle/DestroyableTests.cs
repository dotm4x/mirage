namespace Mirage.Tests.Core.Lifecycle;

using Mirage.Core.Lifecycle;

public class DestroyableTests
{
    [Fact]
    public void Constructor_StartsUndestroyed()
    {
        var destroyable = new TestDestroyable();

        Assert.False(destroyable.Destroyed);
    }

    [Fact]
    public void Destroy_CallsOnDestroyAndMarksObjectAsDestroyed()
    {
        var destroyable = new TestDestroyable();

        destroyable.Destroy();

        Assert.True(destroyable.OnDestroyCalled);
        Assert.True(destroyable.Destroyed);
    }

    [Fact]
    public void Destroy_CallsOnDestroyBeforeMarkingObjectAsDestroyed()
    {
        var destroyable = new TestDestroyable();

        destroyable.Destroy();

        Assert.False(destroyable.WasDestroyedDuringOnDestroy);
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyed_Throws()
    {
        var destroyable = new TestDestroyable();
        destroyable.Destroy();

        Assert.Throws<DestroyedObjectException>(destroyable.Destroy);
    }

    [Fact]
    public void Destroy_WhenOnDestroyThrows_RemainsUndestroyed()
    {
        var exception = new InvalidOperationException("Test exception");
        var destroyable = new TestDestroyable(() => throw exception);

        var result = Record.Exception(destroyable.Destroy);

        Assert.Same(exception, result);
        Assert.False(destroyable.Destroyed);
    }

    [Fact]
    public void ThrowIfDestroyed_WhenObjectIsActive_DoesNotThrow()
    {
        var destroyable = new TestDestroyable();

        var exception = Record.Exception(destroyable.ValidateNotDestroyed);

        Assert.Null(exception);
    }

    [Fact]
    public void ThrowIfDestroyed_WhenObjectIsDestroyed_Throws()
    {
        var destroyable = new TestDestroyable();
        destroyable.Destroy();

        Assert.Throws<DestroyedObjectException>(destroyable.ValidateNotDestroyed);
    }

    private sealed class TestDestroyable(Action? onDestroy = null) : Destroyable
    {
        public bool OnDestroyCalled { get; private set; }

        public bool WasDestroyedDuringOnDestroy { get; private set; }

        protected override void OnDestroy()
        {
            OnDestroyCalled = true;
            WasDestroyedDuringOnDestroy = Destroyed;
            onDestroy?.Invoke();
        }

        public void ValidateNotDestroyed()
        {
            ThrowIfDestroyed();
        }
    }
}
