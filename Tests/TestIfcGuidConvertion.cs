using Bcfier.Data.Utils;
using System;

namespace Tests
{
  public class TestIfcGuidConvertion
  {
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void test_guid_to_ifc_guid()
    {
      // given
      Guid guid = Guid.Parse("effe4d36-9c2f-8ed1-7115-53ca7505126c");

      // when
      var ifcGuid = IfcGuid.ToIfcGuid(guid);

      // then
      var expectedIfcGuid = new string("3l$aqsd2_EqN4LKyfr1H9i");
      Assert.That(expectedIfcGuid, Is.EqualTo(ifcGuid));
    }
  }
}