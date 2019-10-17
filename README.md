# ManualMappingGuard

[![Nuget](https://img.shields.io/nuget/v/ManualMappingGuard)](https://www.nuget.org/packages/ManualMappingGuard)

Applications are often required to map data between different models, e.g. from a domain model to a view model or a DTO. There are libraries like [AutoMapper](https://automapper.org/) for such jobs, however the use of reflection has some drawbacks: navigating and debugging mapping code becomes much harder.

Writing manual mapping code on the other hand is error prone. For instance, it is easy to miss some target properties or source enum values, especially when code changes over time.

ManualMappingGuard therefore provides a Roslyn analyzer which aids writing manual mapping code. It features the following diagnostics:

* Unmapped property detection

## Installation

You need to add the NuGet package `ManualMappingGuard` to the project containing the mapping code. Please note that only the new SDK based projects are supported.

## Usage

### Unmapped property detection

Declare a method as a mapping method by decorating it with `MappingMethodAttribute`. The method must either return a value or own a single parameter decorated with `MappingTargetAttribute`.

The analyzer will report errors for any property with a public setter that is not assigned within the mapping method.

You can exclude properties by adding one or more instances of `UnmappedPropertiesAttribute` to the mapping method and passing the property names to it.

In the example below the analyzer will report that the method `Map` does not map the property `Person.LastName`. The property `Person.Id` is not reported because it is declared as unmapped property.

```csharp
using ManualMappingGuard;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}

public class PersonModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public static class Mapper
{
    [MappingMethod]
    [UnmappedProperties(nameof(Person.Id))]
    public static Person Map(PersonModel model)
    {
        return new Person { FirstName = model.FirstName };
    }
}

/*
  MSBuild Output:

  Program.cs(19, 4): [MMG1001] Property LastName is not mapped.
*/
```

#### Limitations

Please note that the analyzer does not perform any data flow analysis and is satisfied as soon as there is at least one assignment expression for all properties with public setters.

For instance, you can trick the analyzer by the following mapping method. There are assignments to all properties, however the method returns an uninitialized object.

```csharp
[MappingMethod]
public static PersonModel MapToModel(Person person)
{
    var notUsed = new PersonModel
    {
        FirstName = person.FirstName,
        LastName = person.LastName
    };

    return new PersonModel();
}
```