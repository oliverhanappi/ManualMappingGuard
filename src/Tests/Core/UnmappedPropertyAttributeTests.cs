#pragma warning disable CS0618

using NUnit.Framework;

namespace ManualMappingGuard.Core
{
  [TestFixture]
  public class UnmappedPropertyAttributeTests
  {
    [Test]
    public void NullPropertyName_ThrowsException()
    {
      Assert.That(() => new UnmappedPropertyAttribute(null!), Throws.ArgumentNullException);
    }
  }
}
