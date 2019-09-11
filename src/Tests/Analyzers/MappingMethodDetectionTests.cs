using System.Linq;
using ManualMappingGuard.Analyzers.TestInfrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers
{
  [TestFixture]
  public class MappingMethodDetectionTests
  {
    [Test]
    public void IsMappingMethod_RegularAttributeUsage_ReturnsTrue()
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
    public void GetMappingMethodAttributeSyntax_RegularAttributeUsage_ReturnsSyntaxNode()
    {
      var (method, semanticModel) = GetMapMethodDeclaration(@"
        using ManualMappingGuard;

        public class TestClass
        {
          [MappingMethod]
          public object Map() => new object();
        }
      ");

      var attributeSyntax = MappingMethodDetection.GetMappingMethodAttributeSyntax(method, semanticModel);
      Assert.That(attributeSyntax.GetLocation().SourceSpan, Is.EqualTo(new TextSpan(93, 13)));
    }

    [Test]
    public void IsMappingMethod_AsyncMethod_ReturnsTrue()
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
    public void IsMappingMethod_FullyQualifiedAttributeUsage_ReturnsTrue()
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
    public void GetMappingMethodAttributeSyntax_FullyQualifiedAttributeUsage_ReturnsSyntaxNode()
    {
      var (method, semanticModel) = GetMapMethodDeclaration(@"
        public class TestClass
        {
          [global::ManualMappingGuard.MappingMethodAttribute()]
          public object Map() => new object();
        }
      ");

      var attributeSyntax = MappingMethodDetection.GetMappingMethodAttributeSyntax(method, semanticModel);
      Assert.That(attributeSyntax.GetLocation().SourceSpan, Is.EqualTo(new TextSpan(56, 51)));
    }

    [Test]
    public void IsMappingMethod_AliasedAttributeUsage_ReturnsTrue()
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
    public void GetMappingMethodAttributeSyntax_AliasedAttributeUsage_ReturnsSyntaxNode()
    {
      var (method, semanticModel) = GetMapMethodDeclaration(@"
        using XAttribute = ManualMappingGuard.MappingMethodAttribute;

        public class TestClass
        {
          [X]
          public object Map() => new object();
        }
      ");

      var attributeSyntax = MappingMethodDetection.GetMappingMethodAttributeSyntax(method, semanticModel);
      Assert.That(attributeSyntax.GetLocation().SourceSpan, Is.EqualTo(new TextSpan(129, 1)));
    }

    [Test]
    public void IsMappingMethod_NoAttribute_ReturnsFalse()
    {
      var method = GetMapMethod(@"
        public class TestClass
        {
          public object Map() => new object();
        }
      ");

      Assert.That(method.IsMappingMethod(), Is.False);
    }

    [Test]
    public void GetMappingMethodAttributeSyntax_NoAttribute_ReturnsNull()
    {
      var (method, semanticModel) = GetMapMethodDeclaration(@"
        public class TestClass
        {
          public object Map() => new object();
        }
      ");

      var attributeSyntax = MappingMethodDetection.GetMappingMethodAttributeSyntax(method, semanticModel);
      Assert.That(attributeSyntax, Is.Null);
    }

    private IMethodSymbol GetMapMethod(string code)
    {
      var (methodDeclarationSyntax, semanticModel) = GetMapMethodDeclaration(code);
      return (IMethodSymbol) semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
    }

    private (MethodDeclarationSyntax, SemanticModel) GetMapMethodDeclaration(string code)
    {
      var compilation = CompilationUtility.Compile(code);
      var syntaxTree = compilation.SyntaxTrees.Single();
      var semanticModel = compilation.GetSemanticModel(syntaxTree);

      var methodDeclarationSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
      return (methodDeclarationSyntax, semanticModel);
    }
  }
}
