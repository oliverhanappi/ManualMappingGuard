using System.Linq;
using ManualMappingGuard.Analyzers.TestInfrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers.Extensions
{
  [TestFixture]
  public class TypeSymbolExtensionsTests
  {
    [Test]
    public void GetInstanceProperties_ReturnsDeclaredProperties()
    {
      var type = GetType(@"
        public class TestClass
        {
          public int Value { get; set; }
        }
      ");

      AssertProperty(type);
    }

    [Test]
    public void GetInstanceProperties_ReturnsPropertiesFromBaseType()
    {
      var type = GetType(@"
        public class BaseClass
        {
          public int Value { get; set; }
        }

        public class TestClass : BaseClass
        {
        }
      ");

      AssertProperty(type);
    }

    [Test]
    public void GetInstanceProperties_ReturnsOverriddenProperties()
    {
      var type = GetType(@"
        public class BaseClass
        {
          public virtual int Value { get; set; }
        }

        public class TestClass : BaseClass
        {
          public override int Value { get; set; }
        }
      ");

      var property = AssertProperty(type);

      Assert.That(property.IsOverride, Is.True);
    }

    [Test]
    public void GetInstanceProperties_ReturnsLastOverrideOfProperty()
    {
      var type = GetType(@"
        public class BaseClass
        {
          public virtual int Value { get; set; }
        }

        public class IntermediateClass : BaseClass
        {
          public override int Value { get; set; }
        }

        public class TestClass : IntermediateClass
        {
          public override int Value { get; set; }
        }
      ");

      var property = AssertProperty(type);
      Assert.That(property.ContainingType.Name, Is.EqualTo("TestClass"));
    }

    private ITypeSymbol GetType(string code)
    {
      var compilation = CompilationUtility.Compile(code);
      return (ITypeSymbol) compilation.GetSymbolsWithName("TestClass", SymbolFilter.Type).Single();
    }

    private IPropertySymbol AssertProperty(ITypeSymbol type)
    {
      var nonStaticProperties = type.GetInstanceProperties();

      var property = nonStaticProperties.Single();
      Assert.Multiple(() =>
      {
        Assert.That(property.Name, Is.EqualTo("Value"));
        Assert.That(property.Type.SpecialType, Is.EqualTo(SpecialType.System_Int32));
      });

      return property;
    }
  }
}
