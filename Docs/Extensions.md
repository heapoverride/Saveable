## Saveable extensions

Saveable is marked `partial` which allows you to extend it's functionality and keep your modifications in separate files.

#### Example code

In this example we're adding two new methods to handle reading and writing a `Dictonary<TKey, TValue>`.

```cs
// File: Saveable.ReadWriteDictionary.cs
using System;
using System.Collections.Generic;

namespace SaveableDotNet
{
    public abstract partial class Saveable
    {
        /// <summary>
        /// Read a <see cref="Dictionary{TKey, TValue}"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(ReadContext ctx)
        {
            var dict = new Dictionary<TKey, TValue>();

            // Read number of key/value pairs
            var length = ReadInt32(ctx);

            for (int i = 0; i < length; i++)
            {
                // Read key/value pair
                dict.Add(ReadValue<TKey>(ctx), ReadValue<TValue>(ctx));
            }

            return dict;
        }

        /// <summary>
        /// Write a <see cref="Dictionary{TKey, TValue}"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="dict"></param>
        public static void WriteDictionary<TKey, TValue>(WriteContext ctx, Dictionary<TKey, TValue> dict)
        {
            // Write number of key/value pairs
            WriteInt32(ctx, dict.Count);

            foreach (var pair in dict)
            {
                // Write key/value pair
                WriteValue(ctx, pair.Key);
                WriteValue(ctx, pair.Value);
            }
        }
    }
}
```