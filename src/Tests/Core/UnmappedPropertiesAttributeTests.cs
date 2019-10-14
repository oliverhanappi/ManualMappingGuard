using NUnit.Framework;

namespace ManualMappingGuard.Core
{
  [TestFixture]
  public class UnmappedPropertiesAttributeTests
  {
    [Test]
    public void NullPropertyNames_ThrowsException()
    {
      Assert.That(() => new UnmappedPropertiesAttribute((string[]) null!), Throws.ArgumentNullException);
    }
  }
}
