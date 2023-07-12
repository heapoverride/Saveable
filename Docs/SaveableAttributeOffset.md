## Saveable attribute Offset property

Offset property adds an offset to the underlying stream position before property value is read or written.

#### Example
This will insert 4 zero bytes or skip over 4 existing bytes before writing the `Name` string to the stream.

```cs
class Person 
{
    [Saveable(Offset = 4)]
    public string Name { get; set; }

    [Saveable]
    public int Age { get; set; }
}
```