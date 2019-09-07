using System.Linq;
using ManualMappingGuard.Analyzers;
using ManualMappingGuard.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Tests
{
  [TestFixture]
  public class MethodSymbolExtensionsTests
  {
    [Test]
    public void IsMappingMethod_RegularAttributeUsage_ReturnsTrue()
    {
      var (method, _) = GetMapMethod(@"
        using ManualMappingGuard;

        public class TestClass
        {
          [MappingMethod]
          public object Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.True);
    }

    [Test]
    public void IsMappingMethod_AsyncMethod_ReturnsTrue()
    {
      var (method, _) = GetMapMethod(@"
        using ManualMappingGuard;

        public class TestClass
        {
          [MappingMethod]
          public async Task<object> Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.True);
    }

    [Test]
    public void IsMappingMethod_FullyQualifiedAttributeUsage_ReturnsTrue()
    {
      var (method, _) = GetMapMethod(@"
        public class TestClass
        {
          [global::ManualMappingGuard.MappingMethodAttribute()]
          public object Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.True);
    }

    [Test]
    public void IsMappingMethod_AliasedAttributeUsage_ReturnsTrue()
    {
      var (method, _) = GetMapMethod(@"
        using XAttribute = ManualMappingGuard.MappingMethodAttribute;

        public class TestClass
        {
          [X]
          public object Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.True);
    }

    [Test]
    public void IsMappingMethod_NoAttribute_ReturnsFalse()
    {
      var (method, _) = GetMapMethod(@"
        public class TestClass
        {
          public object Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.False);
    }

    [Test]
    public void GetMappingTargetType_Sync_WithReturnValue_ReturnsTypeOfReturnValue()
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
    public void GetMappingTargetType_Async_WithReturnValue_ReturnsTypeOfReturnValue()
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
    public void GetMappingTargetType_Sync_WithoutReturnValue_ReturnsNull()
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
    public void GetMappingTargetType_Async_WithoutReturnValue_ReturnsNull()
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
    public void GetMappingTargetType_AsyncVoid_ReturnsNull()
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
