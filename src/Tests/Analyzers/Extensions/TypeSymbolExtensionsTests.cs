using System.Linq;
using System.Threading.Tasks;
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

    [Test]
    public void InheritsFromOrEquals_SameType_ReturnsTrue()
    {
        Compilation compilation = CompilationUtility.Compile("");

        ITypeSymbol existingType = compilation.GetExistingType<Task>();

        Assert.That(existingType.InheritsFromOrEquals(existingType), Is.True);
    }

    [Test]
    public void InheritsFromOrEquals_DerivedType_ReturnsTrue()
    {
        (Compilation compilation, ITypeSymbol typeSymbol) = this.GetCompilationAndType(@"public class DerivedTask : System.Threading.Tasks.Task
            {
            }", "DerivedTask");

        ITypeSymbol existingType = compilation.GetExistingType<Task>();

        Assert.That(typeSymbol.InheritsFromOrEquals(existingType), Is.True);
    }

    [Test]
    public void InheritsFromOrEquals_NotDerivedType_ReturnsFalse()
    {
        (Compilation compilation, ITypeSymbol typeSymbol) = this.GetCompilationAndType(@"public class DerivedTask
            {
            }", "DerivedTask");

        ITypeSymbol existingType = compilation.GetExistingType<Task>();

        Assert.That(typeSymbol.InheritsFromOrEquals(existingType), Is.False);
    }

    private ITypeSymbol GetType(string code)
    {
        return GetCompilationAndType(code, "TestClass").Item2;
    }

    private (Compilation, ITypeSymbol) GetCompilationAndType(string code, string typeName)
    {
        var compilation = CompilationUtility.Compile(code);
        return (compilation, (ITypeSymbol) compilation.GetSymbolsWithName(typeName, SymbolFilter.Type).Single());
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
