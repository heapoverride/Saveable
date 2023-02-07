# Saveable

Easy to use class library for reading/writing objects from/to binary streams.

# Examples

## A Saveable Fruit class

With a `Saveable` attribute

> **Warning**
> The default `Read` and `Write` instance methods are considerably slower, mainly due to automatic type checking, so if you're working with thousands of Saveable objects, you should look into the second example that overrides the default `Read` and `Write` methods.

```cs
class Fruit : Saveable
{
    // Only primitive types, Saveable and 1-dimensional arrays
    // of primitive types and Saveables can be automatically
    // handled with the Saveable attribute
    [Saveable]
    public string Name { get; set; }

    public Fruit() { }

    public Fruit(string value) {
        Name = value;
    }

    public static implicit operator Fruit(string value)
    {
        return new Fruit(value);
    }
}
```

Without a `Saveable` attribute

```cs
class Fruit : Saveable
{
    public string Name { get; set; }

    public Fruit() { }

    public Fruit(string value) {
        Name = value;
    }

    public static implicit operator Fruit(string value)
    {
        return new Fruit(value);
    }

    protected override void Read(ReadContext ctx)
    {
        // You can still call the base method to automatically 
        // read properties with Saveable attribute
        Name = ReadString(ctx);
    }

    protected override void Write(WriteContext ctx)
    {
        // You can still call the base method to automatically 
        // write properties with Saveable attribute
        WriteString(ctx, Name);
    }
}
```

## Writing an array of Saveable to a binary file

```cs
var fruits = new Fruit[] {
    "Apple",
    "Banana",
    "Mango"
};

var stream = File.Open("Fruits.bin", FileMode.Create, FileAccess.Write);
Saveable.Write(stream, fruits);
```

## Reading a Saveable from a binary file

```cs
var stream = File.Open("Fruits.bin", FileMode.Open, FileAccess.Read);
var fruits = Saveable.ReadArray<Fruit>(stream);

foreach (var fruit in fruits)
{
    Console.WriteLine($"0x{fruit.Position.ToString("X8")} (0x{fruit.Length.ToString("X8")}): {fruit.Name}");
}
```
```
0x00000004 (0x00000009): Apple
0x0000000D (0x0000000A): Banana
0x00000017 (0x00000009): Mango
```

### Reading an array of Saveable from a byte array

```cs
var fruits = Saveable.ReadArray<Fruit>(new byte[] {
    0x03, 0x00, 0x00, 0x00,              // array length = 3
    0x05, 0x00, 0x00, 0x00,              // name length = 5
    0x41, 0x70, 0x70, 0x6C, 0x65,        // name = "Apple"
    0x06, 0x00, 0x00, 0x00,              // name length = 6
    0x42, 0x61, 0x6E, 0x61, 0x6E, 0x61,  // name = "Banana"
    0x05, 0x00, 0x00, 0x00,              // name length = 5
    0x4D, 0x61, 0x6E, 0x67, 0x6F         // name = "Mango"
});
```