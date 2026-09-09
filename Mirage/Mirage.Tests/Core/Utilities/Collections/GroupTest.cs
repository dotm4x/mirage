namespace Mirage.Tests.Core.Utilities.Collections;

using Mirage.Core.Utilities.Collections;
using Xunit;

public class GroupTest
{
    [Fact]
    public void Constructor_WhenNoItemsProvided_CreatesEmptyGroup()
    {
        var group = new Group<int>();

        Assert.Equal(0, group.Count);
        Assert.False(group.Destroyed);
        Assert.Equal(0, group.Limit);
    }

    [Fact]
    public void Constructor_WithItems_AddsAllItems()
    {
        var items = new[] { 1, 2, 3 };

        var group = new Group<int>(items);

        Assert.Equal(3, group.Count);
        Assert.Equal(items, group.ToArray());
    }

    [Fact]
    public void Constructor_WithNegativeLimit_Throws()
    {
        const int limit = -1;

        Assert.Throws<ArgumentOutOfRangeException>((Func<Group<int>>)Action);
        return;

        static Group<int> Action() => new(limit: limit);
    }

    [Fact]
    public void Add_WithItems_AddsItemsAndReturnsThem()
    {
        var group = new Group<int>();
        var items = new[] { 1, 2, 3 };

        var result = group.Add(items);

        Assert.Equal(items, result);
        Assert.Equal(3, group.Count);
        Assert.Equal(items, group.ToArray());
    }

    [Fact]
    public void Add_WhenItemAlreadyExists_Throws()
    {
        var group = new Group<int> { 1 };

        Assert.Throws<InvalidOperationException>((Func<int[]>)Action);
        return;

        int[] Action() => group.Add(1);
    }

    [Fact]
    public void Add_WhenLimitIsReached_RemovesOldestItem()
    {
        var group = new Group<int>(limit: 2) { { 1, 2 }, 3 };

        Assert.Equal(2, group.Count);
        Assert.Equal([2, 3], [.. group]);
    }

    [Fact]
    public void Add_WhenLimitIsOne_KeepsOnlyNewestItem()
    {
        var group = new Group<int>(limit: 1) { 1, 2, 3 };

        Assert.Equal(1, group.Count);
        Assert.Equal([3], [.. group]);
    }

    [Fact]
    public void Add_WhenLimitIsZero_AllowsUnlimitedItems()
    {
        var group = new Group<int>(limit: 0) { { 1, 2, 3, 4, 5 } };

        Assert.Equal(5, group.Count);
        Assert.Equal([1, 2, 3, 4, 5], [.. group]);
    }

    [Fact]
    public void Add_WhenItemIsAdded_FiresOnAdd()
    {
        var group = new Group<int>();
        var received = new List<int>();

        group.OnAdd.Connect(received.Add);

        group.Add(1, 2, 3);

        Assert.Equal([1, 2, 3], received);
    }

    [Fact]
    public void Add_WhenLimitIsReached_FiresOnRemoveForTruncatedItem()
    {
        var group = new Group<int>(limit: 2) { 1, 2 };
        var removed = new List<int>();

        group.OnRemove.Connect(removed.Add);

        group.Add(3);

        Assert.Equal([1], removed);
    }

    [Fact]
    public void Add_WhenLimitIsReached_FiresOnRemoveBeforeOnAdd()
    {
        var group = new Group<int>(limit: 2) { 1, 2 };
        var events = new List<string>();

        group.OnRemove.Connect(item => events.Add($"Remove:{item}"));
        group.OnAdd.Connect(item => events.Add($"Add:{item}"));

        group.Add(3);

        Assert.Equal(["Remove:1", "Add:3"], events);
    }

    [Fact]
    public void Remove_WithExistingItem_RemovesItem()
    {
        var group = new Group<int> { { 1, 2, 3 } };

        group.Remove(2);

        Assert.Equal(2, group.Count);
        Assert.Equal([1, 3], [.. group]);
    }

    [Fact]
    public void Remove_WhenItemDoesNotExist_Throws()
    {
        var group = new Group<int> { 1 };

        Assert.Throws<InvalidOperationException>(Action);
        return;

        void Action() => group.Remove(2);
    }

    [Fact]
    public void Remove_WhenItemExists_FiresOnRemove()
    {
        var group = new Group<int> { { 1, 2, 3 } };
        var removed = new List<int>();

        group.OnRemove.Connect(removed.Add);

        group.Remove(2);

        Assert.Equal([2], removed);
    }

    [Fact]
    public void Remove_WhenMultipleItemsExist_FiresOnRemoveForEachItem()
    {
        var group = new Group<int> { { 1, 2, 3 } };
        var removed = new List<int>();

        group.OnRemove.Connect(removed.Add);

        group.Remove(1, 3);

        Assert.Equal([1, 3], removed);
    }

    [Fact]
    public void Contains_WhenItemExists_ReturnsTrue()
    {
        var group = new Group<int> { 42 };

        var result = group.Contains(42);

        Assert.True(result);
    }

    [Fact]
    public void Contains_WhenItemDoesNotExist_ReturnsFalse()
    {
        var group = new Group<int> { 42 };

        var result = group.Contains(10);

        Assert.False(result);
    }

    [Fact]
    public void ForEach_VisitsEveryItem()
    {
        var group = new Group<int> { { 1, 2, 3 } };

        var visited = new List<int>();

        group.ForEach(visited.Add);

        Assert.Equal(new[] { 1, 2, 3 }, visited);
    }

    [Fact]
    public void ToArray_ReturnsCopyOfItems()
    {
        var group = new Group<int> { { 1, 2, 3 } };

        var result = group.ToArray();
        result[0] = 99;

        Assert.Equal([1, 2, 3], [.. group]);
    }

    [Fact]
    public void ToList_ReturnsCopyOfItems()
    {
        var group = new Group<int> { { 1, 2, 3 } };

        var result = group.ToList();
        result.Clear();

        Assert.Equal(3, group.Count);
        Assert.Equal([1, 2, 3], [.. group]);
    }

    [Fact]
    public void Enumeration_ReturnsItemsInInsertionOrder()
    {
        var group = new Group<int> { { 1, 2, 3 } };

        var result = group.ToList();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var group = new Group<int> { { 1, 2, 3 } };

        group.Clear();

        Assert.Equal(0, group.Count);
        Assert.Empty(group.ToArray());
        Assert.False(group.Destroyed);
    }

    [Fact]
    public void Clear_FiresOnClear()
    {
        var group = new Group<int> { { 1, 2, 3 } };
        var fired = false;

        group.OnClear.Connect(_ => fired = true);

        group.Clear();

        Assert.True(fired);
    }

    [Fact]
    public void Clear_FiresOnRemoveForEveryItem()
    {
        var group = new Group<int> { { 1, 2, 3 } };
        var removed = new List<int>();

        group.OnRemove.Connect(removed.Add);

        group.Clear();

        Assert.Equal([1, 2, 3], removed);
    }

    [Fact]
    public void Clear_FiresOnClearBeforeOnRemove()
    {
        var group = new Group<int> { { 1, 2, 3 } };
        var events = new List<string>();

        group.OnClear.Connect(_ => events.Add("Clear"));
        group.OnRemove.Connect(item => events.Add($"Remove:{item}"));

        group.Clear();

        Assert.Equal(["Clear", "Remove:1", "Remove:2", "Remove:3"], events);
    }

    [Fact]
    public void Destroy_ClearsGroupAndMarksItAsDestroyed()
    {
        var group = new Group<int> { { 1, 2, 3 } };

        group.Destroy();

        Assert.True(group.Destroyed);
        Assert.Equal(0, group.Count);
        Assert.Empty(group.ToArray());
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyed_Throws()
    {
        var group = new Group<int>();
        group.Destroy();

        var action = group.Destroy;

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Add_WhenDestroyed_Throws()
    {
        var group = new Group<int>();
        group.Destroy();

        Assert.Throws<InvalidOperationException>((Func<int[]>)Action);
        return;

        int[] Action() => group.Add(1);
    }

    [Fact]
    public void Remove_WhenDestroyed_Throws()
    {
        var group = new Group<int> { 1 };
        group.Destroy();

        Assert.Throws<InvalidOperationException>(Action);
        return;

        void Action() => group.Remove(1);
    }

    [Fact]
    public void Clear_WhenDestroyed_Throws()
    {
        var group = new Group<int>();
        group.Destroy();

        var action = group.Clear;

        Assert.Throws<InvalidOperationException>(action);
    }
}
