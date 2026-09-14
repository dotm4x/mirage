using Mirage.Core.Lifecycle;

namespace Mirage.Tests.Core.Lifecycle;

public class PoolTest
{
    [Fact]
    public void Acquire_AfterRelease_ReturnsRestoredItem()
    {
        var item = new TestPoolable();
        var pool = new Pool<TestPoolable>(() => item);

        pool.Acquire();
        item.Value = 42;

        pool.Release(item);

        var acquired = pool.Acquire();

        Assert.Same(item, acquired);
        Assert.Equal(0, acquired.Value);
    }

    [Fact]
    public void Acquire_CallsOnAcquire()
    {
        var item = new TestPoolable();
        var pool = new Pool<TestPoolable>(() => item);

        pool.Acquire();

        Assert.Equal(1, item.AcquireCount);
    }

    [Fact]
    public void Acquire_WhenMultipleItemsAreAvailable_ReusesLastReleasedItemFirst()
    {
        var first = new TestPoolable();
        var second = new TestPoolable();

        var pool = new Pool<TestPoolable>(() =>
            ReferenceEquals(first, second) ? new TestPoolable() : first
        );

        var acquiredFirst = pool.Acquire();

        pool.Release(acquiredFirst);

        var acquiredSecond = pool.Acquire();

        Assert.Same(acquiredFirst, acquiredSecond);
    }

    [Fact]
    public void Acquire_WhenMultipleItemsAreNeeded_CreatesEachItemOnlyOnce()
    {
        var created = 0;

        var pool = new Pool<TestPoolable>(() =>
        {
            created++;
            return new TestPoolable();
        });

        var first = pool.Acquire();
        var second = pool.Acquire();

        Assert.NotSame(first, second);
        Assert.Equal(2, created);
    }

    [Fact]
    public void Acquire_WhenPoolHasAvailableItem_ReusesItem()
    {
        var created = 0;
        var pool = new Pool<TestPoolable>(() =>
        {
            created++;
            return new TestPoolable();
        });

        var first = pool.Acquire();
        pool.Release(first);

        var second = pool.Acquire();

        Assert.Same(first, second);
        Assert.Equal(1, created);
    }

    [Fact]
    public void Acquire_WhenPoolIsEmpty_CreatesNewItem()
    {
        var created = 0;
        var pool = new Pool<TestPoolable>(() =>
        {
            created++;
            return new TestPoolable();
        });

        var item = pool.Acquire();

        Assert.NotNull(item);
        Assert.Equal(1, created);
    }

    [Fact]
    public void Release_AllowsItemToBeAcquiredAgain()
    {
        var pool = new Pool<TestPoolable>(() => new TestPoolable());

        var item = pool.Acquire();

        pool.Release(item);

        var acquired = pool.Acquire();

        Assert.Same(item, acquired);
    }

    [Fact]
    public void Release_CallsOnRelease()
    {
        var item = new TestPoolable();
        var pool = new Pool<TestPoolable>(() => item);

        pool.Acquire();
        pool.Release(item);

        Assert.Equal(1, item.ReleaseCount);
    }

    [Fact]
    public void Release_CallsRestore()
    {
        var item = new TestPoolable();
        var pool = new Pool<TestPoolable>(() => item);

        pool.Acquire();
        pool.Release(item);

        Assert.Equal(1, item.RestoreCount);
    }

    [Fact]
    public void Release_RestoresItemBeforeItIsAvailable()
    {
        var item = new TestPoolable();
        var pool = new Pool<TestPoolable>(() => item);

        pool.Acquire();
        item.Value = 42;

        pool.Release(item);

        Assert.Equal(0, item.Value);
    }

    private sealed class TestPoolable : IPoolable
    {
        public int AcquireCount { get; private set; }

        public int ReleaseCount { get; private set; }

        public int RestoreCount { get; private set; }
        public int Value { get; set; }

        public void OnAcquire()
        {
            AcquireCount++;
        }

        public void OnRelease()
        {
            ReleaseCount++;
        }

        public void Restore()
        {
            RestoreCount++;
            Value = 0;
        }
    }
}
