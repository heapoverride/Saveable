## Reading a Saveable

Read a `Person` from a stream.

```cs
// Open a file for reading
var stream = File.Open("Person.bin", FileMode.Open, FileAccess.Read);

// Read a person from a stream
var person = Saveable.Read<Person>(stream);

// Print person details
Console.WriteLine($"{person.Name}, {person.Age}");
```

## Reading an array of Saveable

Read an array of `Person` from a stream.

```cs
// Open a file for reading
var stream = File.Open("Persons.bin", FileMode.Open, FileAccess.Read);

// Read persons from a stream
var persons = Saveable.ReadArray<Person>(stream);

foreach (var person in persons)
{
    // Print person details
    Console.WriteLine($"{person.Name}, {person.Age}");
}
```