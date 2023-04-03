## Writing a Saveable

Write a `Person` to a stream.

```cs
// Create a person object
var person = new Person { Name = "Derek", Age = 27 };

// Open a file for writing
var stream = File.Open("Person.bin", FileMode.Create, FileAccess.Write);

// Write a person to a stream
Saveable.Write(stream, person);
```

## Writing an array of Saveable

Write an array of `Person` to a stream.

```cs
// Create an array of persons
var persons = new Person[] 
{
    new Person { Name = "Derek", Age = 27 },
    new Person { Name = "John", Age = 25 },
    new Person { Name = "Adam", Age = 24 }
};

// Open a file for writing
var stream = File.Open("Person.bin", FileMode.Create, FileAccess.Write);

// Write a persons to a stream
Saveable.Write(stream, persons);
```