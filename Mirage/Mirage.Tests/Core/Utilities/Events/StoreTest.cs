namespace Mirage.Tests.Core.Utilities.Events;

using Mirage.Core.Utilities.Events;
using Xunit;

public class StoreTest
{
    [Fact]
    public void Constructor_WithInitialValue_StoresValue()
    {
        var store = new Store<int>(42);

        Assert.Equal(42, store.Get());
        Assert.False(store.Destroyed);
    }

    [Fact]
    public void Get_ReturnsCurrentValue()
    {
        var store = new Store<int>(10);

        store.Set(20);

        Assert.Equal(20, store.Get());
    }

    [Fact]
    public void Set_WithDifferentValue_UpdatesValue()
    {
        var store = new Store<int>(10);

        store.Set(20);

        Assert.Equal(20, store.Get());
    }

    [Fact]
    public void Set_WithDifferentValue_NotifiesListeners()
    {
        var store = new Store<int>(10);
        var received = 0;

        store.Connect(value => received = value);

        store.Set(20);

        Assert.Equal(20, received);
    }

    [Fact]
    public void Set_WithSameValue_DoesNotUpdateOrNotify()
    {
        var store = new Store<int>(10);
        var calls = 0;

        store.Connect(_ => calls++);

        store.Set(10);

        Assert.Equal(10, store.Get());
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Set_WithMultipleChanges_NotifiesForEachChange()
    {
        var store = new Store<int>(0);
        var received = new List<int>();

        store.Connect(received.Add);

        store.Set(1);
        store.Set(2);
        store.Set(3);

        Assert.Equal([1, 2, 3], received);
    }

    [Fact]
    public void Set_WithSameValueBetweenChanges_DoesNotNotify()
    {
        var store = new Store<int>(0);
        var received = new List<int>();

        store.Connect(received.Add);

        store.Set(1);
        store.Set(1);
        store.Set(2);
        store.Set(2);

        Assert.Equal([1, 2], received);
    }

    [Fact]
    public void Set_WithCustomEquality_WhenValuesAreEqual_DoesNotNotify()
    {
        var store = new Store<string>(
            "hello",
            (current, next) => current.Equals(next, StringComparison.OrdinalIgnoreCase)
        );
        var calls = 0;

        store.Connect(_ => calls++);

        store.Set("HELLO");

        Assert.Equal("hello", store.Get());
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Set_WithCustomEquality_WhenValuesAreDifferent_UpdatesValue()
    {
        var store = new Store<string>(
            "hello",
            (current, next) => current.Equals(next, StringComparison.OrdinalIgnoreCase)
        );

        store.Set("world");

        Assert.Equal("world", store.Get());
    }

    [Fact]
    public void Set_WithCustomEquality_WhenValuesAreDifferent_NotifiesListeners()
    {
        var store = new Store<string>(
            "hello",
            (current, next) => current.Equals(next, StringComparison.OrdinalIgnoreCase)
        );
        var received = string.Empty;

        store.Connect(value => received = value);

        store.Set("world");

        Assert.Equal("world", received);
    }

    [Fact]
    public void Set_WhenStoreIsDestroyed_Throws()
    {
        var store = new Store<int>(42);
        store.Destroy();

        Assert.Throws<InvalidOperationException>(Action);
        return;

        void Action() => store.Set(10);
    }

    [Fact]
    public void Get_WhenStoreIsDestroyed_ReturnsCurrentValue()
    {
        var store = new Store<int>(42);

        store.Destroy();

        Assert.Equal(42, store.Get());
    }

    [Fact]
    public void Set_WhenValueIsReferenceType_UsesDefaultEquality()
    {
        var value = new object();
        var store = new Store<object>(value);
        var calls = 0;

        store.Connect(_ => calls++);

        store.Set(value);

        Assert.Same(value, store.Get());
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Set_WhenValueIsReferenceTypeAndDifferentReference_UpdatesValue()
    {
        var initialValue = new object();
        var newValue = new object();
        var store = new Store<object>(initialValue);

        store.Set(newValue);

        Assert.Same(newValue, store.Get());
    }

    [Fact]
    public void Set_WhenListenerIsDisconnected_DoesNotNotifyListener()
    {
        var store = new Store<int>(0);
        var calls = 0;

        var connection = store.Connect(_ => calls++);

        connection.Disconnect();
        store.Set(42);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void Set_WithPersistentConnection_NotifiesListener()
    {
        var store = new Store<int>(0);
        var calls = 0;

        store.Connect(_ => calls++, persistent: true);

        store.Clear();
        store.Set(42);

        Assert.Equal(1, calls);
    }

    [Fact]
    public void AsReadonly_ReturnsCurrentValue()
    {
        var store = new Store<int>(42);

        var readonlyStore = store.AsReadonly();

        Assert.Equal(42, readonlyStore.Get());
    }

    [Fact]
    public void AsReadonly_ReflectsUpdatedValue()
    {
        var store = new Store<int>(42);
        var readonlyStore = store.AsReadonly();

        store.Set(100);

        Assert.Equal(100, readonlyStore.Get());
    }

    [Fact]
    public void AsReadonly_AllowsListeningToChanges()
    {
        var store = new Store<int>(42);
        var readonlyStore = store.AsReadonly();
        var received = 0;

        readonlyStore.Event.Connect(value => received = value);

        store.Set(100);

        Assert.Equal(100, received);
    }

    [Fact]
    public void AsReadonly_WhenStoreIsDestroyed_CannotConnect()
    {
        var store = new Store<int>(42);
        var readonlyStore = store.AsReadonly();

        store.Destroy();

        Assert.Throws<InvalidOperationException>(Action);
        return;

        void Action() => readonlyStore.Event.Connect(_ => { });
    }

    [Fact]
    public void Destroy_MarksStoreAsDestroyed()
    {
        var store = new Store<int>(42);

        store.Destroy();

        Assert.True(store.Destroyed);
    }

    [Fact]
    public void Destroy_RemovesAllConnections()
    {
        var store = new Store<int>(42);
        var calls = 0;

        store.Connect(_ => calls++);

        store.Destroy();

        Assert.Throws<InvalidOperationException>(() => store.Set(100));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyed_Throws()
    {
        var store = new Store<int>(42);
        store.Destroy();

        var action = store.Destroy;

        Assert.Throws<InvalidOperationException>(action);
    }
}
