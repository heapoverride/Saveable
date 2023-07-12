## Encrypted wrapper

This wrapper can be used to read and write encrypted `Saveable` objects. 

> **Warning**
> Change `key` and `iv` before using.

```cs
using System;
using System.Security.Cryptography;
using SaveableDotNet;

/// <summary>
/// Represents an encrypted wrapper for <see cref="Saveable"/> object
/// </summary>
/// <typeparam name="T"></typeparam>
internal class Encrypted<T> : Saveable where T : Saveable
{
    private T value;

    /// <summary>
    /// Wrapped <see cref="Saveable"/> object
    /// </summary>
    public T Value { get { return value; } }

    private static Aes aes;

    private static byte[] key = new byte[] 
    {
        0x93, 0x30, 0x4B, 0xB6, 0xBB, 0x42, 0xBB, 0xF8, 0x92, 0xF7, 0x76, 0xEE, 0xA4, 0xFE, 0x2E, 0xBA, 
        0x42, 0x7A, 0x5A, 0xA0, 0x55, 0x2C, 0x8D, 0x6B, 0x96, 0x1B, 0x8F, 0x39, 0xC4, 0x95, 0x57, 0x11
    };
    
    private static byte[] iv = new  byte[] 
    {
        0x8A, 0xD4, 0xA6, 0x6E, 0xA5, 0x16, 0xCC, 0xA5, 0xF3, 0x9D, 0x5D, 0x24, 0x77, 0xAB, 0x0C, 0x34
    };

    static Encrypted() 
    {
        aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;
        aes.Key = key;
        aes.IV = iv;
    }

    /// <summary>
    /// Instantiate new encrypted <see cref="Saveable"/> object
    /// </summary>
    /// <param name="value"></param>
    public Encrypted(T value)
    {
        this.value = value;
    }

    protected override void Read(ReadContext ctx)
    {
        using (var decryptor = aes.CreateDecryptor())
        {
            using (var cryptoStream = new CryptoStream(ctx.Stream, decryptor, CryptoStreamMode.Read, true))
            {
                value = Read<T>(cryptoStream);
            }
        }
    }

    protected override void Write(WriteContext ctx)
    {
        using (var encryptor = aes.CreateEncryptor())
        {
            using (var cryptoStream = new CryptoStream(ctx.Stream, encryptor, CryptoStreamMode.Write, true))
            {
                Write(cryptoStream, value);
            }
        }
    }

    public static implicit operator Encrypted<T>(T value) => new Encrypted<T>(value);
    public static implicit operator T(Encrypted<T> value) => value.Value;
}
```