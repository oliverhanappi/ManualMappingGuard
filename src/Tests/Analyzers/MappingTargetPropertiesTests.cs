using System.Linq;
using ManualMappingGuard.Analyzers.TestInfrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers
{
  [TestFixture]
  public class MappingTargetPropertiesTests
  {
    [Test]
    public void PublicProperty_PublicSetter_IncludesProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value { get; set; }
        }
      ");
      
      AssertMappingTargetProperties(type, "Value");
    }

    [Test]
    public void PublicProperty_PrivateSetter_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value { get; private set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void PublicProperty_ProtectedSetter_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value { get; protected set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void PublicProperty_InternalSetter_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value { get; internal set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void PublicProperty_ProtectedInternalSetter_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value { get; protected internal set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void PublicProperty_GetterOnly_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value { get; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void PublicProperty_SetterOnly_IncludesProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value { set { } }
        }
      ");
      
      AssertMappingTargetProperties(type, "Value");
    }

    [Test]
    public void ProtectedProperty_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          protected int Value { get; set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void InternalProperty_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          internal int Value { get; set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void ProtectedInternalProperty_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          protected internal int Value { get; set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void PrivateProperty_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          private int Value { get; set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void StaticProperty_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public static int Value { get; set; }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void Indexer_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int this[int index]
          {
            get => 0;
            set { }
          }
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }
    
    [Test]
    public void VirtualProperty_IncludesProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public virtual int Value { get; set; }
        }
      ");
      
      AssertMappingTargetProperties(type, "Value");
    }

    [Test]
    public void AbstractProperty_IncludesProperty()
    {
      var type = GetType(@"
        public abstract class DTO
        {
          public abstract int Value { get; set; }
        }
      ");
      
      AssertMappingTargetProperties(type, "Value");
    }

    [Test]
    public void Event_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public event EventHandler Event;
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void Field_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value;
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void Method_DoesNotIncludeProperty()
    {
      var type = GetType(@"
        public class DTO
        {
          public int Value() => 0;
        }
      ");
      
      AssertNoMappingTargetProperties(type);
    }

    [Test]
    public void DeclaredOnBaseClass_IncludesProperty()
    {
      var type = GetType(@"
        public class Base
        {
          public int Value { get; set; }
        }

        public class DTO : Base
        {
        }
      ");
      
      AssertMappingTargetProperties(type, "Value");
    }

    [Test]
    public void OverriddenFromBaseClass_IncludesPropertyOnlyOnce()
    {
      var type = GetType(@"
        public class Base
        {
          public virtual int Value { get; set; }
        }

        public class DTO : Base
        {
          public override int Value { get; set; }
        }
      ");
      
      AssertMappingTargetProperties(type, "Value");
    }

    [Test]
    public void AbstractImplementedFromBaseClass_IncludesPropertyOnlyOnce()
    {
      var type = GetType(@"
        public class Base
        {
          public abstract int Value { get; set; }
        }

        public class DTO : Base
        {
          public override int Value { get; set; }
        }
      ");
      
      AssertMappingTargetProperties(type, "Value");
    }

    private ITypeSymbol GetType(string code)
    {
      var compilation = CompilationUtility.Compile(code);
      return (ITypeSymbol) compilation.GetSymbolsWithName("DTO", SymbolFilter.Type).Single();
    }

    private void AssertMappingTargetProperties(ITypeSymbol type, params string[] expectedPropertyNames)
    {
      var mappingTargetProperties = type.GetMappingTargetProperties();
      var mappingTargetPropertyNames = mappingTargetProperties.Select(p => p.Name).ToList();

      Assert.That(mappingTargetPropertyNames, Is.EquivalentTo(expectedPropertyNames));
    }

    private void AssertNoMappingTargetProperties(ITypeSymbol type) => AssertMappingTargetProperties(type);
  }
}
