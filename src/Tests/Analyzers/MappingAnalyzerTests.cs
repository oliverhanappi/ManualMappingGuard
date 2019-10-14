using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ManualMappingGuard.Analyzers.TestInfrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers
{
  [TestFixture]
  public class MappingAnalyzerTests : AnalyzerTestBase<MappingAnalyzer>
  {
    protected override string CodeTemplate => @"
      using System;
      using ManualMappingGuard;

      namespace TestNamespace
      {{
        public static class TestClass
        {{
          {0}
        }}
      }}
    ";

    [Test]
    public async Task MissingMappingTargetType_ReturnsDiagnostic()
    {
      var diagnostics = await Analyze(@"
        [MappingMethod]
        public void Map()
        {
        }
      ");

      AssertMissingMappingTargetType(diagnostics);
    }

    [Test]
    public async Task PropertyAssignments_DetectsUnmappedProperty()
    {
      var diagnostics = await Analyze(@"
        public class Person
        {
          public string FirstName { get; set; }
          public string LastName { get; set; }
        }

        [MappingMethod]
        public Person Map()
        {
          var person = new Person();
          person.FirstName = ""Test"";
          return person;
        }
      ");

      AssertUnmappedProperties(diagnostics, "LastName");
    }

    [Test]
    public async Task ObjectInitializer_DetectsUnmappedProperty()
    {
      var diagnostics = await Analyze(@"
        public class Person
        {
          public string FirstName { get; set; }
          public string LastName { get; set; }
        }

        [MappingMethod]
        public Person Map()
        {
            return new Person
            {
                FirstName = ""Test""
            };
        }
      ");

      AssertUnmappedProperties(diagnostics, "LastName");
    }

    [Test]
    public async Task BaseClassProperty_DetectsUnmappedProperty()
    {
      var diagnostics = await Analyze(@"
        public class Base
        {
          public string LastName { get; set; }
        }

        public class Person : Base
        {
          public string FirstName { get; set; }
        }

        [MappingMethod]
        public Person Map()
        {
            return new Person
            {
                FirstName = ""Test""
            };
        }
      ");

      AssertUnmappedProperties(diagnostics, "LastName");
    }

    [Test]
    public async Task OverriddenProperty_DetectsMapped()
    {
      var diagnostics = await Analyze(@"
        public class Base
        {
          public virtual string LastName { get; set; }
        }

        public class Person : Base
        {
          public string FirstName { get; set; }
          public override string LastName { get; set; }
        }

        [MappingMethod]
        public Person Map()
        {
          var person = new Person();
          person.FirstName = ""Test"";
          ((Base) person).LastName = ""Test"";
          return person;
        }
      ");

      AssertNoUnmappedProperties(diagnostics);
    }

    [Test]
    public async Task UnmappedProperty_ExcludedSingle_DoesNotReportMissingProperty()
    {
      var diagnostics = await Analyze(@"
        public class Person
        {
          public int Id { get; set; }
          public string FirstName { get; set; }
          public string LastName { get; set; }
        }

        [MappingMethod]
        [UnmappedProperty(nameof(Person.Id))]
        public Person Map() => new Person { FirstName = ""Test"" };
      ");

      AssertUnmappedProperties(diagnostics, "LastName");
    }

    [Test]
    public async Task UnmappedProperty_ExcludedMultiple_DoesNotReportMissingProperty()
    {
      var diagnostics = await Analyze(@"
        public class Person
        {
          public int Id { get; set; }
          public string FirstName { get; set; }
          public string LastName { get; set; }
        }

        [MappingMethod]
        [UnmappedProperties(nameof(Person.Id))]
        public Person Map() => new Person { FirstName = ""Test"" };
      ");

      AssertUnmappedProperties(diagnostics, "LastName");
    }

    [Test]
    public async Task UnmappedProperty_MultipleExcludedIndividually_DoesNotReportMissingProperties()
    {
      var diagnostics = await Analyze(@"
        public class Person
        {
          public const string IdPropertyName = ""Id"";

          public int Id { get; set; }
          public string FirstName { get; set; }
          public string LastName { get; set; }
        }

        [MappingMethod]
        [UnmappedProperty(Person.IdPropertyName)]
        [UnmappedProperty(""LastName"")]
        public Person Map() => new Person { FirstName = ""Test"" };
      ");

      AssertNoUnmappedProperties(diagnostics);
    }

    [Test]
    public async Task UnmappedProperty_MultipleExcludedAtOnce_DoesNotReportMissingProperties()
    {
      var diagnostics = await Analyze(@"
        public class Person
        {
          public const string IdPropertyName = ""Id"";

          public int Id { get; set; }
          public string FirstName { get; set; }
          public string LastName { get; set; }
        }

        [MappingMethod]
        [UnmappedProperties(Person.IdPropertyName, ""LastName"")]
        public Person Map() => new Person { FirstName = ""Test"" };
      ");

      AssertNoUnmappedProperties(diagnostics);
    }

    [Test]
    public async Task NonExistingExcludedProperty_Single_DoesNotFail()
    {
      var diagnostics = await Analyze(@"
        public class Person { }

        [MappingMethod]
        [UnmappedProperty(""NotExisting"")]
        public Person Map() => new Person();
      ");

      Assert.That(diagnostics, Is.Empty);
    }

    [Test]
    public async Task NonExistingExcludedProperty_Multiple_DoesNotFail()
    {
      var diagnostics = await Analyze(@"
        public class Person { }

        [MappingMethod]
        [UnmappedProperties(""NotExisting1"", ""NonExisting2"")]
        public Person Map() => new Person();
      ");

      Assert.That(diagnostics, Is.Empty);
    }

    private void AssertMissingMappingTargetType(ImmutableArray<Diagnostic> diagnostics)
    {
      var expectedMessages = new[]
      {
        "Error MMG0001: Unable to determine target type of mapping. Ensure that this method either returns a value or has a single parameter decorated with MappingTargetAttribute."
      };

      var actualMessages = diagnostics.Select(FormatDiagnostic);
      Assert.That(actualMessages, Is.EquivalentTo(expectedMessages));
    }

    private void AssertUnmappedProperties(ImmutableArray<Diagnostic> diagnostics, params string[] propertyNames)
    {
      var expectedMessages = propertyNames
        .Select(p => $"Error MMG1001: Property {p} is not mapped.")
        .ToList();

      var actualMessages = diagnostics.Select(FormatDiagnostic);

      Assert.That(actualMessages, Is.EquivalentTo(expectedMessages));
    }

    private void AssertNoUnmappedProperties(ImmutableArray<Diagnostic> diagnostics)
    {
      Assert.That(diagnostics, Is.Empty);
    }

    private string FormatDiagnostic(Diagnostic diagnostic)
    {
      return $"{diagnostic.Severity} {diagnostic.Id}: {diagnostic.GetMessage(CultureInfo.InvariantCulture)}";
    }
  }
}
