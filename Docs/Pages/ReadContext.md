## ReadContext

Using a `ReadContext` to read a string from a stream.

```cs
// Open a file for reading
var stream = File.Open("Strings.bin", FileMode.Open, FileAccess.Read);

// Create a read context
var ctx = new Saveable.ReadContext(stream);

// Read a string
Console.WriteLine(Saveable.ReadString(ctx));
```