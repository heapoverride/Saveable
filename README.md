# Saveable

Easy to use class library for reading/writing objects from/to binary streams.

# Examples

## Saveable Fruit class

With `Saveable` attribute

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

Without `Saveable` attribute

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

    public override void Read(BinaryReader reader)
    {
        // You can still call the base method to automatically 
        // read properties with Saveable attribute
        Name = ReadString(reader);
    }

    public override void Write(BinaryWriter writer)
    {
        // You can still call the base method to automatically 
        // write properties with Saveable attribute
        WriteString(writer, Name);
    }
}
```

## Writing array of Saveable to binary file

```cs
var fruits = new Fruit[] {
    "Apple",
    "Banana",
    "Mango"
};

var stream = File.Open("Fruits.bin", FileMode.Create, FileAccess.Write);
var writer = new BinaryWriter(stream);

Saveable.Write(writer, fruits);
```

## Reading Saveable from binary file

```cs
var stream = File.Open("Fruits.bin", FileMode.Open, FileAccess.Read);
var reader = new BinaryReader(stream);

var fruits = Saveable.ReadArray<Fruit>(reader);

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

### Reading array of Saveable from byte array

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