using Bcfier.Localization;
using NUnit.Framework;
using System.Globalization;

namespace Tests
{
  [TestFixture]
  public class TestLocalization
  {
    private CultureInfo _originalCulture;

    [SetUp]
    public void SetUp()
    {
      _originalCulture = LocValueGetter.Culture;
    }

    [TearDown]
    public void TearDown()
    {
      LocValueGetter.Culture = _originalCulture;
    }

    [Test]
    public void default_culture_follows_current_ui_culture()
    {
      // given
      var expected = CultureInfo.CurrentUICulture;

      // when
      var result = LocValueGetter.Culture;

      // then
      Assert.That(result.Name, Is.EqualTo(expected.Name));
    }

    [Test]
    public void set_culture_stores_culture_info()
    {
      // when
      LocValueGetter.SetCulture("ru-RU");

      // then
      Assert.That(LocValueGetter.Culture, Is.EqualTo(CultureInfo.GetCultureInfo("ru-RU")));
    }

    [Test]
    public void set_culture_null_or_empty_falls_back_to_invariant()
    {
      // when
      LocValueGetter.SetCulture(null);

      // then
      Assert.That(LocValueGetter.Culture, Is.EqualTo(CultureInfo.InvariantCulture));
    }

    [Test]
    public void get_returns_localized_string_for_russian()
    {
      // given
      LocValueGetter.SetCulture("ru-RU");

      // when
      var result = LocValueGetter.Get("Error");

      // then
      Assert.That(result, Is.EqualTo("Ошибка"));
    }

    [Test]
    public void get_returns_localized_string_for_english()
    {
      // given
      LocValueGetter.SetCulture("en-US");

      // when
      var result = LocValueGetter.Get("Error");

      // then
      Assert.That(result, Is.EqualTo("Error"));
    }

    [Test]
    public void loc_extension_with_key_resolves_via_loc_value_getter()
    {
      // given
      LocValueGetter.SetCulture("ru-RU");
      var extension = new LocExtension("Error");

      // when
      var result = extension.ProvideValue(null);

      // then
      Assert.That(result, Is.EqualTo("Ошибка"));
    }

    [Test]
    public void loc_extension_property_key_is_configurable()
    {
      // given
      LocValueGetter.SetCulture("en-US");
      var extension = new LocExtension { Key = "OK" };

      // when
      var result = extension.ProvideValue(null);

      // then
      Assert.That(result, Is.EqualTo("OK"));
    }
  }
}