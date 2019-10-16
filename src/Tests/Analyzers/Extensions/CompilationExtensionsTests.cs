using System.Threading.Tasks;
using ManualMappingGuard.Analyzers.TestInfrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers.Extensions
{
  [TestFixture]
  public class CompilationExtensionsTests
  {
    private Compilation _compilation = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _compilation = CompilationUtility.Compile("");
    }
    
    [Test]
    public void GetExistingTypeByMetadataName_ExistingType_ReturnsTypeSymbol()
    {
      var type = (INamedTypeSymbol) _compilation.GetExistingTypeByMetadataName("System.Threading.Tasks.Task`1");

      Assert.That(type.Name, Is.EqualTo("Task"));
      Assert.That(type.ContainingNamespace.ToString(), Is.EqualTo("System.Threading.Tasks"));
      Assert.That(type.IsGenericType, Is.True);
    }
    
    [Test]
    public void GetExistingTypeByMetadataName_NonExistingType_ThrowsException()
    {
      Assert.That(() => _compilation.GetExistingTypeByMetadataName("SomeNameSpace.NonExistingClass"),
        Throws.ArgumentException.With.Message.StartsWith("Failed to get type SomeNameSpace.NonExistingClass."));
    }
    
    [Test]
    public void GetExistingType_Generic_Referenced_ReturnsTypeSymbol()
    {
      var type = _compilation.GetExistingType<int>();
      Assert.That(type.SpecialType, Is.EqualTo(SpecialType.System_Int32));
    }
    
    [Test]
    public void GetExistingType_Generic_NotReferenced_ThrowsException()
    {
      Assert.That(() => _compilation.GetExistingType<TestFixtureAttribute>(),
        Throws.ArgumentException.With.Message.StartsWith("Failed to get type NUnit.Framework.TestFixtureAttribute"));
    }
    
    [Test]
    public void GetExistingType_NonGeneric_Referenced_ReturnsTypeSymbol()
    {
      var type = (INamedTypeSymbol) _compilation.GetExistingType(typeof(Task<>));

      Assert.That(type.Name, Is.EqualTo("Task"));
      Assert.That(type.ContainingNamespace.ToString(), Is.EqualTo("System.Threading.Tasks"));
      Assert.That(type.IsGenericType, Is.True);
    }
    
    [Test]
    public void GetExistingType_NonGeneric_NotReferenced_ThrowsException()
    {
      Assert.That(() => _compilation.GetExistingType(typeof(TestFixtureAttribute)),
        Throws.ArgumentException.With.Message.StartsWith("Failed to get type NUnit.Framework.TestFixtureAttribute"));
    }

    [Test]
    public void TypeIsDerivedOrEqual_SameType_ReturnsTrue()
    {
      var existingType = _compilation.GetExistingType<Task>();

      var result = _compilation.TypeIsDerivedOrEqual(existingType, existingType);

      Assert.That(result, Is.True);
    }

    [Test]
    public void TypeIsDerivedOrEqual_DerivedType_ReturnsTrue()
    {
      var baseType = _compilation.GetExistingType<Task>();
      var derivedType = _compilation.GetExistingType(typeof(Task<>));

      bool result = _compilation.TypeIsDerivedOrEqual(derivedType, baseType);

      Assert.That(result, Is.True);
    }

    [Test]
    public void TypeIsDerivedOrEqual_DerivedType_WrongHierarchy_ReturnsFalse()
    {
      var baseType = _compilation.GetExistingType<Task>();
      var derivedType = _compilation.GetExistingType(typeof(Task<>));

      var result = _compilation.TypeIsDerivedOrEqual(baseType, derivedType);

      Assert.That(result, Is.False);
    }

    [Test]
    public void TypeIsDerivedOrEqual_NonDerivedType_ReturnsFalse()
    {
      var baseType = _compilation.GetExistingType<Task>();
      var nonDerivedType = _compilation.GetExistingType<int>();

      var result = _compilation.TypeIsDerivedOrEqual(nonDerivedType, baseType);

      Assert.That(result, Is.False);
    }
  }
}
