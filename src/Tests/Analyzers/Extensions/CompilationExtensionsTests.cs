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
  }
}
