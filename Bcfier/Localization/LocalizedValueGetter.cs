using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Bcfier.Localization
{
  public class LocValueGetter
  {
    private static readonly Lazy<ResourceManager> _resourceManager = new Lazy<ResourceManager>(() =>
    {
      var assembly = Assembly.GetExecutingAssembly();
      return new ResourceManager("BCFier.Localization.Strings", assembly);
    });

    private static CultureInfo _culture = CultureInfo.CurrentUICulture;

    public static CultureInfo Culture
    {
      get { return _culture; }
      set { _culture = value ?? CultureInfo.InvariantCulture; }
    }

    public static void SetCulture(string cultureName)
    {
      Culture = string.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : new CultureInfo(cultureName);
    }

    public static string Get(string key)
    {
      return _resourceManager.Value.GetString(key, Culture);
    }
  }
}