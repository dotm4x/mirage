using Mirage.Common.Collections;
using Mirage.Common.Lifecycle;

namespace Mirage.Tests.Common.Collections;

public class ReactiveDictionaryTest
{
    [Fact]
    public void Add_WhenDestroyed_Throws()
    {
        var dictionary = new ReactiveDictionary<string, int>();
        dictionary.Destroy();

        Assert.Throws<DestroyedObjectException>(() => dictionary.Add("one", 1));
    }

    [Fact]
    public void Add_WhenEntryIsAdded_AddsEntry()
    {
        var dictionary = new ReactiveDictionary<string, int> { { "one", 1 } };

        Assert.Single(dictionary);
        Assert.Equal(1, dictionary["one"]);
    }

    [Fact]
    public void Add_WhenEntryIsAdded_FiresOnAdd()
    {
        var dictionary = new ReactiveDictionary<string, int>();
        var added = new List<KeyValuePair<string, int>>();

        dictionary.OnAdd.Connect(added.Add);

        dictionary.Add("one", 1);

        Assert.Equal([new KeyValuePair<string, int>("one", 1)], added);
    }

    [Fact]
    public void Add_WhenKeyAlreadyExists_Throws()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        Assert.Throws<ArgumentException>(() => dictionary.Add("one", 2));
    }

    [Fact]
    public void Clear_FiresOnClear()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };
        var fired = false;

        dictionary.OnClear.Connect(_ => fired = true);

        dictionary.Clear();

        Assert.True(fired);
    }

    [Fact]
    public void Clear_FiresOnClearBeforeOnRemove()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };
        var events = new List<string>();

        dictionary.OnClear.Connect(_ => events.Add("Clear"));
        dictionary.OnRemove.Connect(item => events.Add($"Remove:{item.Key}"));

        dictionary.Clear();

        Assert.Equal(
            ["Clear", "Remove:one", "Remove:two", "Remove:three"],
            events
        );
    }

    [Fact]
    public void Clear_FiresOnRemoveForEveryEntry()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };
        var removed = new List<KeyValuePair<string, int>>();

        dictionary.OnRemove.Connect(removed.Add);

        dictionary.Clear();

        Assert.Equal(
            [
                new KeyValuePair<string, int>("one", 1),
                new KeyValuePair<string, int>("two", 2),
                new KeyValuePair<string, int>("three", 3)
            ],
            removed
        );
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        dictionary.Clear();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void Clear_WhenDestroyed_Throws()
    {
        var dictionary = new ReactiveDictionary<string, int>();
        dictionary.Destroy();

        Assert.Throws<DestroyedObjectException>(dictionary.Clear);
    }

    [Fact]
    public void Constructor_WhenNoItemsProvided_CreatesEmptyReactiveDictionary()
    {
        var dictionary = new ReactiveDictionary<string, int>();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void Constructor_WithItems_AddsAllEntries()
    {
        var items = new[]
        {
            new KeyValuePair<string, int>("one", 1),
            new KeyValuePair<string, int>("two", 2),
            new KeyValuePair<string, int>("three", 3)
        };

        var dictionary = new ReactiveDictionary<string, int>(items);

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(items, dictionary.ToArray());
    }

    [Fact]
    public void ContainsKey_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        Assert.False(dictionary.ContainsKey("two"));
    }

    [Fact]
    public void ContainsKey_WhenKeyExists_ReturnsTrue()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        Assert.True(dictionary.ContainsKey("one"));
    }

    [Fact]
    public void ContainsValue_WhenValueDoesNotExist_ReturnsFalse()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        Assert.False(dictionary.ContainsValue(2));
    }

    [Fact]
    public void ContainsValue_WhenValueExists_ReturnsTrue()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        Assert.True(dictionary.ContainsValue(1));
    }

    [Fact]
    public void Destroy_ClearsReactiveDictionary()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        dictionary.Destroy();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void Enumeration_ReturnsEntriesInInsertionOrder()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        Assert.Equal(
            [
                new KeyValuePair<string, int>("one", 1),
                new KeyValuePair<string, int>("two", 2),
                new KeyValuePair<string, int>("three", 3)
            ],
            dictionary.ToArray()
        );
    }

    [Fact]
    public void ForEach_VisitsEveryEntry()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };
        var visited = new List<KeyValuePair<string, int>>();

        dictionary.ForEach((key, value) => visited.Add(new KeyValuePair<string, int>(key, value)));

        Assert.Equal(
            [
                new KeyValuePair<string, int>("one", 1),
                new KeyValuePair<string, int>("two", 2),
                new KeyValuePair<string, int>("three", 3)
            ],
            visited
        );
    }

    [Fact]
    public void Indexer_Get_WhenKeyDoesNotExist_Throws()
    {
        var dictionary = new ReactiveDictionary<string, int>();

        Assert.Throws<KeyNotFoundException>(() => dictionary["one"]);
    }

    [Fact]
    public void Indexer_Get_WhenKeyExists_ReturnsValue()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        Assert.Equal(1, dictionary["one"]);
    }

    [Fact]
    public void Indexer_Set_WhenDestroyed_Throws()
    {
        var dictionary = new ReactiveDictionary<string, int>();
        dictionary.Destroy();

        Assert.Throws<DestroyedObjectException>(() => dictionary["one"] = 1);
    }

    [Fact]
    public void Indexer_Set_WhenKeyDoesNotExist_AddsEntry()
    {
        var dictionary = new ReactiveDictionary<string, int>();

        dictionary["one"] = 1;

        Assert.Single(dictionary);
        Assert.Equal(1, dictionary["one"]);
    }

    [Fact]
    public void Indexer_Set_WhenKeyDoesNotExist_DoesNotFireOnUpdate()
    {
        var dictionary = new ReactiveDictionary<string, int>();
        var updated = new List<(
            KeyValuePair<string, int> Previous,
            KeyValuePair<string, int> Current
        )>();

        dictionary.OnUpdate.Connect(updated.Add);

        dictionary["one"] = 1;

        Assert.Empty(updated);
    }

    [Fact]
    public void Indexer_Set_WhenKeyDoesNotExist_FiresOnAdd()
    {
        var dictionary = new ReactiveDictionary<string, int>();
        var added = new List<KeyValuePair<string, int>>();

        dictionary.OnAdd.Connect(added.Add);

        dictionary["one"] = 1;

        Assert.Equal([new KeyValuePair<string, int>("one", 1)], added);
    }

    [Fact]
    public void Indexer_Set_WhenKeyExists_DoesNotFireOnAdd()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };
        var added = new List<KeyValuePair<string, int>>();

        dictionary.OnAdd.Connect(added.Add);

        dictionary["one"] = 2;

        Assert.Empty(added);
    }

    [Fact]
    public void Indexer_Set_WhenKeyExists_FiresOnUpdate()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };
        var updated = new List<(
            KeyValuePair<string, int> Previous,
            KeyValuePair<string, int> Current
        )>();

        dictionary.OnUpdate.Connect(updated.Add);

        dictionary["one"] = 2;

        Assert.Equal(
            [
                (
                    new KeyValuePair<string, int>("one", 1),
                    new KeyValuePair<string, int>("one", 2)
                )
            ],
            updated
        );
    }

    [Fact]
    public void Indexer_Set_WhenKeyExists_UpdatesValue()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        dictionary["one"] = 2;

        Assert.Single(dictionary);
        Assert.Equal(2, dictionary["one"]);
        Assert.False(dictionary.ContainsValue(1));
    }

    [Fact]
    public void Remove_WhenDestroyed_Throws()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };
        dictionary.Destroy();

        Assert.Throws<DestroyedObjectException>(() => dictionary.Remove("one"));
    }

    [Fact]
    public void Remove_WhenKeyDoesNotExist_DoesNotFireOnRemove()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };
        var removed = new List<KeyValuePair<string, int>>();

        dictionary.OnRemove.Connect(removed.Add);

        dictionary.Remove("two");

        Assert.Empty(removed);
    }

    [Fact]
    public void Remove_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        var result = dictionary.Remove("two");

        Assert.False(result);
        Assert.Single(dictionary);
    }

    [Fact]
    public void Remove_WhenKeyExists_FiresOnRemove()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };
        var removed = new List<KeyValuePair<string, int>>();

        dictionary.OnRemove.Connect(removed.Add);

        dictionary.Remove("two");

        Assert.Equal([new KeyValuePair<string, int>("two", 2)], removed);
    }

    [Fact]
    public void Remove_WhenKeyExists_ReturnsTrue()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        var result = dictionary.Remove("one");

        Assert.True(result);
        Assert.Empty(dictionary);
    }

    [Fact]
    public void ToArray_ReturnsCopyOfEntries()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        var result = dictionary.ToArray();
        result[0] = new KeyValuePair<string, int>("changed", 99);

        Assert.Equal(
            [
                new KeyValuePair<string, int>("one", 1),
                new KeyValuePair<string, int>("two", 2),
                new KeyValuePair<string, int>("three", 3)
            ],
            dictionary.ToArray()
        );
    }

    [Fact]
    public void ToList_ReturnsCopyOfEntries()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        var result = dictionary.ToList();
        result.Clear();

        Assert.Equal(3, dictionary.Count);
    }

    [Fact]
    public void TryGetValue_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        var result = dictionary.TryGetValue("two", out var value);

        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGetValue_WhenKeyExists_ReturnsTrueAndValue()
    {
        var dictionary = new ReactiveDictionary<string, int>
        {
            { "one", 1 }
        };

        var result = dictionary.TryGetValue("one", out var value);

        Assert.True(result);
        Assert.Equal(1, value);
    }
}