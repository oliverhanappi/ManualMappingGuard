using System.Linq;
using ManualMappingGuard.Analyzers.TestInfrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers
{
  [TestFixture]
  public class MappingTargetTypeDetectionTests
  {
    [Test]
    public void SyncMethod_WithReturnValue_ReturnsTypeOfReturnValue()
    {
      var (method, compilation) = GetMapMethod(@"
        public class TestClass
        {
          public int Map() => 0;
        }
      ");

      var targetType = method.GetMappingTargetType(compilation);
      Assert.That(targetType.SpecialType, Is.EqualTo(SpecialType.System_Int32));
    }

    [Test]
    public void AsyncMethod_WithReturnValue_ReturnsTypeOfReturnValue()
    {
      var (method, compilation) = GetMapMethod(@"
        using System.Threading.Tasks;

        public class TestClass
        {
          public async Task<int> Map() => 0;
        }
      ");

      var targetType = method.GetMappingTargetType(compilation);
      Assert.That(targetType.SpecialType, Is.EqualTo(SpecialType.System_Int32));
    }

    [Test]
    public void SyncMethod_WithoutReturnValue_ReturnsNull()
    {
      var (method, compilation) = GetMapMethod(@"
        public class TestClass
        {
          public void Map() { }
        }
      ");

      var targetType = method.GetMappingTargetType(compilation);
      Assert.That(targetType, Is.Null);
    }

    [Test]
    public void AsyncMethod_WithoutReturnValue_ReturnsNull()
    {
      var (method, compilation) = GetMapMethod(@"
        using System.Threading.Tasks;

        public class TestClass
        {
          public async Task Map() { }
        }
      ");

      var targetType = method.GetMappingTargetType(compilation);
      Assert.That(targetType, Is.Null);
    }

    [Test]
    public void AsyncVoid_ReturnsNull()
    {
      var (method, compilation) = GetMapMethod(@"
        using System.Threading.Tasks;

        public class TestClass
        {
          public async void Map() { }
        }
      ");

      var targetType = method.GetMappingTargetType(compilation);
      Assert.That(targetType, Is.Null);
    }

    private (IMethodSymbol, Compilation) GetMapMethod(string code)
    {
      var compilation = CompilationUtility.Compile(code);
      var method = (IMethodSymbol) compilation.GetSymbolsWithName("Map", SymbolFilter.Member).Single();

      return (method, compilation);
    }
  }
}
