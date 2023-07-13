## Saveable class

A simple `Person` class that inherits from `Saveable`. Properties are marked with `Saveable` attribute and will be automatically handled.

```cs
using SaveableNET;

class Person : Saveable 
{
    [Saveable]
    public string Name { get; set; }

    [Saveable]
    public int Age { get; set; }

    public Person() {}
}
```

Sometimes you need to handle an unsupported type so using `Saveable` attribute atleast for that type is not an option. This example shows how you can manually handle your properties. This is by default faster if you need the extra performance since there is no type checking.

```cs
using SaveableNET;

class Person : Saveable 
{
    public string Name;
    public int Age;

    public Person() {}

    protected override Read(ReadContext ctx) 
    {
        Name = ReadString(ctx);
        Age = ReadInt32(ctx);
    }

    protected override Write(WriteContext ctx) 
    {
        WriteString(ctx, Name);
        WriteInt32(ctx, Age);
    }
}
```