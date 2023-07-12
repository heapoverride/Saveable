## Supported types

Supported types are all primitive data types, Saveable and 1d arrays of those.

## Custom type support

Example methods to read and write `Dictionary<K, V>` that is not supported by default.

```cs
static Dictionary<K, V> ReadDictionary<K, V>(Saveable.ReadContext ctx)
{
    var dict = new Dictionary<K, V>();

    // Read number of key/value pairs
    var length = Saveable.ReadInt32(ctx);

    for (int i = 0; i < length; i++)
    {
        // Read key/value pair
        dict.Add(Saveable.ReadValue<K>(ctx), Saveable.ReadValue<V>(ctx));
    }

    return dict;
}
```

```cs
static void WriteDictionary<K, V>(Saveable.WriteContext ctx, Dictionary<K, V> dict)
{
    // Write number of key/value pairs
    Saveable.WriteInt32(ctx, dict.Count);

    foreach (var pair in dict)
    {
        // Write key/value pair
        Saveable.WriteValue(ctx, pair.Key);
        Saveable.WriteValue(ctx, pair.Value);
    }
}
```