using System.Linq;
using ManualMappingGuard.Analyzers.TestInfrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers
{
  [TestFixture]
  public class MappingMethodDetectionTests
  {
    [Test]
    public void RegularAttributeUsage_ReturnsTrue()
    {
      var method = GetMapMethod(@"
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
    public void AsyncMethod_ReturnsTrue()
    {
      var method = GetMapMethod(@"
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
    public void FullyQualifiedAttributeUsage_ReturnsTrue()
    {
      var method = GetMapMethod(@"
        public class TestClass
        {
          [global::ManualMappingGuard.MappingMethodAttribute()]
          public object Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.True);
    }

    [Test]
    public void AliasedAttributeUsage_ReturnsTrue()
    {
      var method = GetMapMethod(@"
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
    public void NoAttribute_ReturnsFalse()
    {
      var method = GetMapMethod(@"
        public class TestClass
        {
          public object Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.False);
    }

    private IMethodSymbol GetMapMethod(string code)
    {
      var compilation = CompilationUtility.Compile(code);
      var method = (IMethodSymbol) compilation.GetSymbolsWithName("Map", SymbolFilter.Member).Single();

      return method;
    }
  }
}
