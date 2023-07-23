## WriteContext

Using a `WriteContext` to write a string to a stream.

```cs
// Open a file for writing
var stream = File.Open("Strings.bin", FileMode.Create, FileAccess.Write);

// Create a write context
var ctx = new Saveable.WriteContext(stream);

// Write a string
Saveable.Write(ctx, "Hello, world!");
```